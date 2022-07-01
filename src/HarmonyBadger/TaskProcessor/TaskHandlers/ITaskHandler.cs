using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// Interface for a handler that can execute a <see cref="ITask"/>.
/// </summary>
public interface ITaskHandler
{
    /// <summary>
    /// Executes the given task.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="log">A helper used for logging telemetry.</param>
    Task HandleAsync(ITask task, ILogger log);
}