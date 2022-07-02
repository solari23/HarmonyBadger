using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// Implements the abstract foundation of polymorphic task handling.
/// Concrete implementations inheriting from this class will implement
/// implement specialized handling of different types of tasks.
/// </summary>
/// <typeparam name="TTask">
/// The type of task that the speciailized implementation inherting from this class handles.
/// </typeparam>
public abstract class TaskHandlerBase<TTask> : ITaskHandler
    where TTask : class, ITask
{
    /// <summary>
    /// Creates a new instance of the <see cref="TaskHandlerBase{TTask}"/> class.
    /// </summary>
    public TaskHandlerBase(IConfigProvider configProvider)
    {
        this.ConfigProvider = configProvider;
    }

    protected IConfigProvider ConfigProvider { get; }

    /// <inheritdoc />
    public async Task HandleAsync(ITask task, ILogger log)
    {
        if (task is not TTask specializedTask)
        {
            throw new InvalidOperationException(
                $"Task handler of type '{this.GetType().Name}' was invoked to handle a task of type {task.GetType().Name} but can only handle {typeof(TTask).Name}.");
        }

        await this.HandleAsync(specializedTask, log);
    }

    /// <summary>
    /// Specialized implementation for handling the specific type of task.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="log">A helper for telemetry logging.</param>
    protected abstract Task HandleAsync(TTask task, ILogger log);
}
