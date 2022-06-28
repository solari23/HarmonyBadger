using System;
using System.Threading.Tasks;

using HarmonyBadgerFunctionApp.TaskModel;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace HarmonyBadgerFunctionApp.Scheduler;

/// <summary>
/// The HarmonyBadger_Scheduler function implements the logic for
/// evaluating <see cref="ScheduledTask"/> configurations and queueing
/// up tasks to be executed when their schedule is due.
/// </summary>
public class SchedulerFunction
{
    private const string EveryHourAt50MinsTrigger = "0 50 */1 * * *";
    private const string Every30SecondsTrigger = "*/30 * * * * *";

    /// <summary>
    /// Creates a new instance of the <see cref="SchedulerFunction"/> class.
    /// </summary>
    public SchedulerFunction(
        IScheduledTaskConfigLoader taskConfigLoader,
        IClock clock)
    {
        this.TaskConfigLoader = taskConfigLoader;
        this.Clock = clock;
    }

    private IScheduledTaskConfigLoader TaskConfigLoader { get; }

    private IClock Clock { get; }

    /// <summary>
    /// The entry point for the HarmonyBadger_Scheduler function.
    /// </summary>
    [FunctionName("HarmonyBadger_Scheduler")]
    public async Task RunAsync(
        [TimerTrigger(EveryHourAt50MinsTrigger)] TimerInfo myTimer,
        ILogger log,
        ExecutionContext context)
    {
        // Load the Scheduled Task configs.
        var configs = await this.TaskConfigLoader.LoadScheduledTasksAsync(log, context);

        // Evaluate all the configs' schedules.
        var scheduleChecker = new ScheduleTriggerChecker(context.InvocationId.ToString());
        this.GetTriggerCheckTimeRange(out DateTimeOffset startTimeUtc, out DateTimeOffset endTimeUtc);
        var triggeredTasks = scheduleChecker.GetTriggeredTasks(configs, startTimeUtc, endTimeUtc);

        // TODO: Publish the triggered tasks to the task queue.
        // TODO: Improve logging.
        log.LogInformation($"[{this.Clock.UtcNow}][L:{this.Clock.LocalNow}] Timer triggered, found {configs.Count} config files, resulting in {triggeredTasks.Count} tasks being triggered");
    }

    private void GetTriggerCheckTimeRange(out DateTimeOffset startUtc, out DateTimeOffset endUtc)
    {
        // Check the scheduled task executions for the next hour.
        var utcNow = this.Clock.UtcNow;
        startUtc = utcNow
            .AddHours(1)
            .AddMinutes(-utcNow.Minute)
            .AddSeconds(-utcNow.Second)
            .AddMilliseconds(-utcNow.Millisecond);
        endUtc = startUtc.AddHours(1).AddSeconds(-1);
    }
}
