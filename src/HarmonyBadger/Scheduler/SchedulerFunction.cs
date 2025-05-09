using System.Text.Json;

using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels;

namespace HarmonyBadger.Scheduler;

/// <summary>
/// The HarmonyBadger_Scheduler function implements the logic for
/// evaluating <see cref="ScheduledTask"/> configurations and queueing
/// up tasks to be executed when their schedule is due.
/// </summary>
public class SchedulerFunction
{
    private const string EveryHourAt50MinsTrigger = "0 50 * * * *";

#if DEBUG
    private const bool LauchImmediately = true;
#else
    private const bool LauchImmediately = false;
#endif

    /// <summary>
    /// Creates a new instance of the <see cref="SchedulerFunction"/> class.
    /// </summary>
    public SchedulerFunction(
        IConfigProvider configProvider,
        IClock clock,
        ILogger<SchedulerFunction> logger,
        IAzureClientFactory<QueueServiceClient> queueClientFactory)
    {
        this.ConfigProvider = configProvider;
        this.Clock = clock;
        this.Logger = logger;

        this.TaskQueueClient = queueClientFactory
            .CreateClient(Constants.DefaultStorageClientName)
            .GetQueueClient(Constants.TaskQueueName);
    }

    private IConfigProvider ConfigProvider { get; }

    private IClock Clock { get; }

    private ILogger<SchedulerFunction> Logger { get; }

    private QueueClient TaskQueueClient { get; }

    /// <summary>
    /// The entry point for the HarmonyBadger_Scheduler function.
    /// </summary>
    [Function("HarmonyBadger_Scheduler")]
    public async Task RunAsync(
        [TimerTrigger(EveryHourAt50MinsTrigger, RunOnStartup = LauchImmediately)] TimerInfo timer,
        FunctionContext context)
        => await this.RunSchedulerAsync(context);

    /// <summary>
    /// The entry point for the HarmonyBadger_Scheduler_Force function.
    /// </summary>
    [Function("HarmonyBadger_Scheduler_Force")]
    public async Task ForceRunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "forceScheduler")] HttpRequest request,
        FunctionContext context)
        => await this.RunSchedulerAsync(context);

    private async Task RunSchedulerAsync(FunctionContext context)
    {
        var logContext = new SchedulerLogContext(context.InvocationId, this.Clock);

        // Load the Scheduled Task configs.
        var configs = await this.ConfigProvider.GetScheduledTasksAsync(this.Logger);
        logContext.LoadedTaskConfigs = configs;

        // Evaluate all the configs' schedules.
        var scheduleChecker = new ScheduleTriggerChecker(logContext);
        this.GetTriggerCheckTimeRange(out DateTimeOffset startTimeUtc, out DateTimeOffset endTimeUtc);
        var triggeredTasks = scheduleChecker.GetTriggeredTasks(configs, startTimeUtc, endTimeUtc);
        logContext.TriggeredTasks = triggeredTasks;

        // Publish the triggered tasks to the task queue.
        // The task processor function will pick them up and execute them.
        var failedEnqueueCount = 0;

        foreach (var task in triggeredTasks)
        {
            try
            {
                var taskJson = JsonSerializer.Serialize(task);

                var now = this.Clock.UtcNow;
                TimeSpan? delay = task.TriggerTimeUtc <= now || LauchImmediately
                    ? null
                    : task.TriggerTimeUtc - now;

                await this.TaskQueueClient.SendMessageAsync(taskJson, delay);
                this.Logger.LogMetric(Constants.MetricNames.TaskQueued, 1);
            }
            catch (Exception ex)
            {
                failedEnqueueCount++;
                this.Logger.LogError(ex, $"Failed to enqueue task [{task.ToLogString()}]. It will be dropped.");
                this.Logger.LogMetric(Constants.MetricNames.EnqueueTaskFailed, 1);
            }
        }

        logContext.FailedTaskEnqueueCount = failedEnqueueCount;
        logContext.PublishTo(this.Logger);
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
