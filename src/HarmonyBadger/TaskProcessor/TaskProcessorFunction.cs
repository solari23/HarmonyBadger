using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels;
using HarmonyBadger.TaskProcessor.TaskHandlers;

namespace HarmonyBadger.TaskProcessor;

/// <summary>
/// The HarmonyBadger_TaskProcessor function reads <see cref="TaskActivationDetails"/>
/// items from an Azure Storage Queue and executes them.
/// </summary>
public class TaskProcessorFunction
{
    /// <summary>
    /// Creates a new instance of the <see cref="TaskProcessorFunction"/> class.
    /// </summary>
    public TaskProcessorFunction(
        IClock clock,
        ITaskHandlerFactory taskHandlerFactory,
        ILogger<TaskProcessorFunction> logger)
    {
        this.Clock = clock;
        this.TaskHandlerFactory = taskHandlerFactory;
        this.Logger = logger;
    }

    private IClock Clock { get; }

    private ITaskHandlerFactory TaskHandlerFactory { get; }

    private ILogger<TaskProcessorFunction> Logger { get; }

    /// <summary>
    /// The entry points for the HarmonyBadger_TaskProcessor function.
    /// </summary>
    [Function("HarmonyBadger_TaskProcessor")]
    public async Task RunAsync(
        [QueueTrigger(Constants.TaskQueueName)] QueueMessage queueMessage,
        FunctionContext context)
    {
        var logContext = new TaskProcessorLogContext(context.InvocationId, this.Clock);

        try
        {
            await this.HandleMessageAsync(queueMessage, logContext);
            this.Logger.LogMetric(Constants.MetricNames.TaskExecuted, 1);
        }
        catch (Exception)
        {
            // Handling the task failed; error detail logging is handled in HandleMessageAsync.
            // We'll swallow the error -- Azure Function-level retry is not needed.
            this.Logger.LogMetric(Constants.MetricNames.TaskExecutionFailed, 1);
        }

        logContext.PublishTo(this.Logger);
    }

    private async Task HandleMessageAsync(
        QueueMessage queueMessage,
        TaskProcessorLogContext logContext)
    {
        TaskActivationDetails task;

        try
        {
            task = queueMessage.Body.ToObjectFromJson<TaskActivationDetails>();
            logContext.Task = task;
        }
        catch (Exception e)
        {
            var error = $"Deserializing message {queueMessage.MessageId} failed.";
            this.Logger.LogError(e, error);
            logContext.TaskProcessingFailureReason = error;
            throw;
        }

        try
        {
            var handler = this.TaskHandlerFactory.CreateHandler(task.Task.TaskKind);
            await handler.HandleAsync(task.Task, this.Logger);
        }
        catch (Exception e)
        {
            var error = $"Executing task {task.ToLogString()} (from message {queueMessage.MessageId}) failed.";
            this.Logger.LogError(e, error);
            logContext.TaskProcessingFailureReason = error;
            throw;
        }
    }
}
