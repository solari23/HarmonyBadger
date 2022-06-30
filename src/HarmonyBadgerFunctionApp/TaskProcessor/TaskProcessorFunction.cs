using System;
using System.Threading.Tasks;

using Azure.Storage.Queues.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using HarmonyBadgerFunctionApp.Scheduler;
using HarmonyBadgerFunctionApp.TaskModel;

namespace HarmonyBadgerFunctionApp.TaskProcessor;

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
        IScheduledTaskConfigLoader taskConfigLoader,
        IClock clock)
    {
        this.TaskConfigLoader = taskConfigLoader;
        this.Clock = clock;
    }

    private IScheduledTaskConfigLoader TaskConfigLoader { get; }

    private IClock Clock { get; }

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

        TaskActivationDetails task = null;

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
        }

        // TODO: Implement task handlers to execute tasks.
        await Task.Yield();

        logContext.Publish(log);
    }
}
