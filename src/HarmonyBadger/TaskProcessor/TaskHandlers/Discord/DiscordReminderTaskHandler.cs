using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels.Tasks;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// Handler for <see cref="TaskKind.DiscordReminder"/> tasks.
/// </summary>
public class DiscordReminderTaskHandler : TaskHandlerBase<DiscordReminderTask>
{
    /// <inheritdoc />
    protected override Task HandleAsync(DiscordReminderTask task, ILogger log)
    {
        // TODO: Implement task handling.
        log.LogInformation("[HANDLING DISCORD REMINDER TASK]");
        return Task.CompletedTask;
    }
}
