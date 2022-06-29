using System;
using System.Text.Json;
using System.Threading.Tasks;

using Azure.Storage.Queues;
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
    private const string EveryHourAt50MinsTrigger = "0 50 * * * *";

#if DEBUG
    private const bool TriggerOnStartup = true;
#else
    private const bool TriggerOnStartup = false;
#endif

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
        [TimerTrigger(EveryHourAt50MinsTrigger, RunOnStartup = TriggerOnStartup)] TimerInfo myTimer,
        [Queue(Constants.TaskQueueName)] QueueClient taskQueueClient,
        ILogger log,
        ExecutionContext context)
    {
        var logContext = new SchedulerLogContext(context.InvocationId, this.Clock);

        // Load the Scheduled Task configs.
        var configs = await this.TaskConfigLoader.LoadScheduledTasksAsync(log, context);
        logContext.LoadedTaskConfigs = configs;

        // Evaluate all the configs' schedules.
        var scheduleChecker = new ScheduleTriggerChecker(logContext);
        this.GetTriggerCheckTimeRange(out DateTimeOffset startTimeUtc, out DateTimeOffset endTimeUtc);
        var triggeredTasks = scheduleChecker.GetTriggeredTasks(configs, startTimeUtc, endTimeUtc);
        logContext.TriggeredTasks = triggeredTasks;

        // Publish the triggered tasks to the task queue.
        // The executor function will pick them up and execute them.
        var failedEnqueueCount = 0;

        foreach (var task in triggeredTasks)
        {
            try
            {
                var taskJson = JsonSerializer.Serialize(task);

                var now = this.Clock.UtcNow;
                TimeSpan? delay = task.TriggerTimeUtc <= now
                    ? null
                    : task.TriggerTimeUtc - now;

                await taskQueueClient.SendMessageAsync(taskJson, delay);
            }
            catch (Exception ex)
            {
                failedEnqueueCount++;
                log.LogError(ex, $"Failed to enqueue task [{task.ToLogString()}]. It will be dropped.");
                log.LogMetric(Constants.MetricNames.EnqueueTaskFailed, 1);
            }
        }

        logContext.FailedTaskEnqueueCount = failedEnqueueCount;
        logContext.Publish(log);
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
        endUtc = startUtc.AddHours(1);
    }
}
