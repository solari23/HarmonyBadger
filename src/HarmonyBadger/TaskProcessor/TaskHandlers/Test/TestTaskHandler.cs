using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels;
using HarmonyBadger.ConfigModels.Tasks;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// A handler for <see cref="TaskKind.Test"/> tasks.
/// </summary>
public class TestTaskHandler : TaskHandlerBase<TestTask>
{
    /// <summary>
    /// Creates a new instance of the <see cref="TestTaskHandler"/> class.
    /// </summary>
    public TestTaskHandler(IConfigProvider configProvider) : base(configProvider)
    {
        // Empty.
    }

    /// <inheritdoc />
    protected override Task HandleAsync(TestTask task, ILogger log)
    {
        log.LogInformation($"[EXECUTING TEST TASK] {task.DebugMessage}");
        return Task.CompletedTask;
    }
}
