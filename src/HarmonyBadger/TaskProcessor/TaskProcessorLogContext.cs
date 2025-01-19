using System.Text;

using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels;

namespace HarmonyBadger.TaskProcessor;

/// <summary>
/// Tracks data during <see cref="TaskProcessorFunction"/> execution for eventual logging.
/// </summary>
public class TaskProcessorLogContext
{
    /// <summary>
    /// Creates a new instance of the <see cref="TaskProcessorLogContext"/> class.
    /// </summary>
    /// <param name="invocationId">The Azure Function invocation ID.</param>
    /// <param name="clock">A clock that provides the current time.</param>
    public TaskProcessorLogContext(string invocationId, IClock clock)
    {
        this.InvocationId = invocationId;
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
    /// The task that was queued for processing.
    /// </summary>
    public TaskActivationDetails Task { private get; set; }

    /// <summary>
    /// If task processing fails, a reason to be recorded in the canonical log.
    /// </summary>
    public string TaskProcessingFailureReason { private get; set; }

    /// <summary>
    /// Formats and publishes log data.
    /// </summary>
    /// <param name="logger">The logging utility.</param>
    public void PublishTo(ILogger logger)
    {
        var builder = new StringBuilder();
        builder.Append($"[{this.ExecutionTimeUtc:s}][L:{this.ExecutionTimeLocal}][TaskProcessor]");

        if (this.Task is not null)
        {
            builder.Append($" Received task {this.Task.ToLogString()}.");
        }

        if (!string.IsNullOrEmpty(this.TaskProcessingFailureReason))
        {
            builder.Append($" Processing task failed because: {this.TaskProcessingFailureReason}.");
        }

        logger.LogInformation(builder.ToString());
    }
}
