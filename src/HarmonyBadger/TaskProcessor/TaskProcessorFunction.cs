using Azure.Storage.Queues.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels;
using HarmonyBadger.TaskProcessor.TaskHandlers;

using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

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
        ITaskHandlerFactory taskHandlerFactory)
    {
        this.Clock = clock;
        this.TaskHandlerFactory = taskHandlerFactory;
    }

    private IClock Clock { get; }

    private ITaskHandlerFactory TaskHandlerFactory { get; }

    /// <summary>
    /// The entry points for the HarmonyBadger_TaskProcessor function.
    /// </summary>
    [FunctionName("HarmonyBadger_TaskProcessor")]
    public async Task RunAsync(
        [QueueTrigger(Constants.TaskQueueName)] QueueMessage queueMessage,
        ILogger log,
        ExecutionContext context)
    {
        var logContext = new TaskProcessorLogContext(context.InvocationId, this.Clock);

        try
        {
            await this.HandleMessageAsync(queueMessage, log, logContext);
        }
        catch (Exception)
        {
            // Handling the task failed; logging is handled in HandleMessageAsync.
            // We'll swallow the error -- Azure Function-level retry is not needed.
        }

        logContext.Publish(log);
    }

    private async Task HandleMessageAsync(
        QueueMessage queueMessage,
        ILogger log,
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
            log.LogError(e, error);
            logContext.TaskProcessingFailureReason = error;
            throw;
        }

        try
        {
            var handler = this.TaskHandlerFactory.CreateHandler(task.Task.TaskKind);
            await handler.HandleAsync(task.Task, log);
        }
        catch (Exception e)
        {
            var error = $"Executing task {task.ToLogString()} (from message {queueMessage.MessageId}) failed.";
            log.LogError(e, error);
            logContext.TaskProcessingFailureReason = error;
            throw;
        }
    }
}
