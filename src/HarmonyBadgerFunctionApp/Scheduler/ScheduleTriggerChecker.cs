using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyBadgerFunctionApp.TaskModel;
using NCrontab;

namespace HarmonyBadgerFunctionApp.Scheduler;

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
    public ScheduleTriggerChecker(string functionInvocationId)
    {
        this.FunctionInvocationId = functionInvocationId;
    }

    private string FunctionInvocationId { get; }

    /// <summary>
    /// Checks the given <see cref="ScheduledTask"/> configurations and returns the tasks
    /// that are triggered between the given start and end times.
    /// </summary>
    /// <param name="scheduledTasks">The <see cref="ScheduledTask"/> configurations.</param>
    /// <param name="startUtc">The start of the time range to check.</param>
    /// <param name="endUtc">The end of the time range to check.</param>
    /// <returns>The tasks that are triggered.</returns>
    public IReadOnlyCollection<TriggeredTask> GetTriggeredTasks(
        IEnumerable<ScheduledTask> scheduledTasks,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc)
    {
        var triggeredTasks = new List<TriggeredTask>();

        var cronParseOptions = new CrontabSchedule.ParseOptions
        {
            IncludingSeconds = false,
        };

        var startLocal = TimeHelper.ConvertToLocal(startUtc).DateTime;
        var endLocal = TimeHelper.ConvertToLocal(endUtc).DateTime;

        foreach (var scheduledTask in scheduledTasks.Where(t => t.IsEnabled))
        {
            var triggerTimesUtc = scheduledTask.Schedule
                .SelectMany(sched => sched.ToCronExpressions()) // Convert the schedule to a cron expression strings
                .Select(cronExpr => CrontabSchedule.Parse(cronExpr, cronParseOptions)) // Convert cron expression strings to cron objects
                .SelectMany(c => c.GetNextOccurrences(startLocal, endLocal)) // Evaluate the cron objects for the specified timespan
                .OrderBy(time => time)
                .Take(Constants.MaxTriggersPerSchedule) // Limit how many times the task can trigger
                .Select(time => TimeHelper.ConvertToUtc(time)); // Convert the timestamps to UTC

            if (triggerTimesUtc.Any())
            {
                triggeredTasks.AddRange(triggerTimesUtc.Select(time => new TriggeredTask
                {
                    TriggerId = TriggeredTask.CreateTriggerId(scheduledTask.Checksum, time),
                    TriggerTimeUtc = time,
                    ScheduleConfigName = scheduledTask.ConfigFileName,
                    ScheduleConfigChecksum = scheduledTask.Checksum,
                    EvaluatingFunctionInvocationId = this.FunctionInvocationId,
                    EvaluationTimeUtc = DateTime.UtcNow,
                }));
            }
        }

        return triggeredTasks;
    }
}
