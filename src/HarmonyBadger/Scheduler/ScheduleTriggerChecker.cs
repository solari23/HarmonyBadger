using NCrontab;

using HarmonyBadger.ConfigModels;

namespace HarmonyBadger.Scheduler;

/// <summary>
/// Helper class that checks <see cref="ScheduledTask"/> objects
/// to see if their schedules indicate that their task should be
/// triggered.
/// </summary>
public class ScheduleTriggerChecker
{
    /// <summary>
    /// Creates a new instance of the <see cref="ScheduleTriggerChecker"/> class.
    /// </summary>
    /// <param name="functionInvocationId">The current function execution invocation ID.</param>
    public ScheduleTriggerChecker(SchedulerLogContext logContext)
    {
        this.LogContext = logContext;
    }

    private SchedulerLogContext LogContext { get; }

    /// <summary>
    /// Checks the given <see cref="ScheduledTask"/> configurations and returns the tasks
    /// that are triggered between the given start and end times.
    /// </summary>
    /// <param name="scheduledTasks">The <see cref="ScheduledTask"/> configurations.</param>
    /// <param name="startUtc">The inclusive start of the time range to check.</param>
    /// <param name="endUtc">The exclusive end of the time range to check.</param>
    /// <returns>The tasks that are triggered.</returns>
    public IReadOnlyCollection<TaskActivationDetails> GetTriggeredTasks(
        IEnumerable<ScheduledTask> scheduledTasks,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc)
    {
        var triggeredTasks = new List<TaskActivationDetails>();

        var cronParseOptions = new CrontabSchedule.ParseOptions
        {
            IncludingSeconds = false,
        };

        var startLocal = TimeHelper.ConvertToLocal(startUtc).DateTime;
        this.LogContext.TriggerCheckTimeStart = startLocal;

        // NCrontab will not report the start time as an occurrence when calling GetNextOccurrences.
        // Adjust by 1 second to make the start time inclusive.
        startLocal = startLocal.AddSeconds(-1);

        var endLocal = TimeHelper.ConvertToLocal(endUtc).DateTime;
        this.LogContext.TriggerCheckTimeEnd = endLocal;

        // NCrontab will report the end time as an occurrence when calling GetNextOcurrences.
        // Adjust by 1 second to make the end time exclusive.
        endLocal = endLocal.AddSeconds(-1);

        foreach (var scheduledTask in scheduledTasks.Where(t => t.IsEnabled))
        {
            var triggerTimesUtc = scheduledTask.Schedule
                .SelectMany(sched => sched.ToCronExpressions()) // Convert the schedule to a cron expression strings
                .Where(scheduleString => !string.IsNullOrEmpty(scheduleString))
                .Select(cronExpr => CrontabSchedule.Parse(cronExpr, cronParseOptions)) // Convert cron expression strings to cron objects
                .SelectMany(c => c.GetNextOccurrences(startLocal, endLocal)) // Evaluate the cron objects for the specified timespan
                .OrderBy(time => time)
                .Take(Constants.MaxTriggersPerSchedule) // Limit how many times the task can trigger
                .Select(time => TimeHelper.ConvertToUtc(time)) // Convert the timestamps to UTC
                .Distinct() // De-dup in case two schedules fire for the same time.
                .ToList();

            if (triggerTimesUtc.Any())
            {
                triggeredTasks.AddRange(triggerTimesUtc.Select(time => new TaskActivationDetails
                {
                    TriggerId = TaskActivationDetails.CreateTriggerId(scheduledTask.Checksum, time),
                    TriggerTimeUtc = time,
                    ScheduleConfigName = scheduledTask.ConfigFileName,
                    ScheduleConfigChecksum = scheduledTask.Checksum,
                    EvaluatingFunctionInvocationId = this.LogContext.InvocationId,
                    EvaluationTimeUtc = DateTime.UtcNow,
                    Task = scheduledTask.Task,
                }));
            }
        }

        return triggeredTasks;
    }
}
