using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels;
using HarmonyBadger.ConfigModels.Tasks;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// A handler for <see cref="TaskKind.Test"/> tasks.
/// </summary>
public class TestTaskHandler : TaskHandlerBase<TestTask>
{
    public TestTaskHandler(ITemplateEngine templateEngine)
    {
        this.TemplateEngine = templateEngine;
    }

    private ITemplateEngine TemplateEngine { get; }

    /// <inheritdoc />
    protected override async Task HandleAsync(TestTask task, ILogger log)
    {
        var message = await this.TemplateEngine.RenderTemplatedMessageAsync(task);
        log.LogInformation($"[EXECUTING TEST TASK] {message}");
    }
}
