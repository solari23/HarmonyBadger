using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyBadgerFunctionApp.TaskModel;
using Microsoft.Extensions.Logging;

namespace HarmonyBadgerFunctionApp.Scheduler;

/// <summary>
/// Tracks data during <see cref="SchedulerFunction"/> execution for eventual logging.
/// </summary>
public class SchedulerLogContext
{
    /// <summary>
    /// Creates a new instance of the <see cref="SchedulerLogContext"/> class.
    /// </summary>
    /// <param name="invocationId">The Azure Function invocation ID.</param>
    /// <param name="clock">A clock that provides the current time.</param>
    public SchedulerLogContext(Guid invocationId, IClock clock)
    {
        this.InvocationId = invocationId.ToString();
        this.ExecutionTimeUtc = clock.UtcNow.DateTime;
        this.ExecutionTimeLocal = clock.LocalNow.DateTime;
    }

    /// <summary>
    /// The executing Azure Function invocation ID.
    /// </summary>
    public string InvocationId { get; }

    /// <summary>
    /// The time the function executed (UTC).
    /// </summary>
    public DateTime ExecutionTimeUtc { get; }

    /// <summary>
    /// The time the function executed (local).
    /// </summary>
    public DateTime ExecutionTimeLocal { get; }

    /// <summary>
    /// The start of the time range used to search for triggered schedules.
    /// </summary>
    public DateTime TriggerCheckTimeStart { private get; set; }

    /// <summary>
    /// The end of the time range used to search for triggered schedules.
    /// </summary>
    public DateTime TriggerCheckTimeEnd { private get; set; }

    /// <summary>
    /// The <see cref="ScheduledTask"/> configurations loaded during execution.
    /// </summary>
    public IReadOnlyCollection<ScheduledTask> LoadedTaskConfigs { private get; set; }

    /// <summary>
    /// The tasks that were triggered during execution.
    /// </summary>
    public IReadOnlyCollection<TaskActivationDetails> TriggeredTasks { private get; set; }

    /// <summary>
    /// The number of <see cref="TriggeredTasks"/> that we failed to enqueue.
    /// </summary>
    public int FailedTaskEnqueueCount { private get; set; }

    /// <summary>
    /// Formats and publishes log data.
    /// </summary>
    /// <param name="logger">The logging utility.</param>
    public void Publish(ILogger logger)
    {
        var builder = new StringBuilder();
        builder.Append($"[{this.ExecutionTimeUtc:s}][L:{this.ExecutionTimeLocal}][Scheduler]");
        builder.Append(
            $" Loaded {this.LoadedTaskConfigs.Count} schedule configurations ({this.LoadedTaskConfigs.Count(c => c.IsEnabled)} enabled).");
        builder.Append(
            $" {this.TriggeredTasks.Count} tasks triggered for the period between [{this.TriggerCheckTimeStart} -> {this.TriggerCheckTimeEnd}].");

        if (this.FailedTaskEnqueueCount > 0)
        {
            builder.Append($" Failed to enqueue {this.FailedTaskEnqueueCount} tasks.");
        }

        logger.LogInformation(builder.ToString());

        // Log the tasks that were triggered.
        builder.Clear();
        builder.Append("Triggered|");
        builder.Append(string.Join('|', this.TriggeredTasks.Select(t => t.ToLogString())));
        logger.LogInformation(builder.ToString());
    }
}
