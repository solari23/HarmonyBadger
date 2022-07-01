using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels.Tasks;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// Handler for <see cref="TaskKind.DiscordReminder"/> tasks.
/// </summary>
public class DiscordReminderTaskHandler : TaskHandlerBase<DiscordReminderTask>
{
    /// <summary>
    /// Creates a new instance of the <see cref="DiscordReminderTaskHandler"/> class.
    /// </summary>
    public DiscordReminderTaskHandler(IConfigProvider configProvider) : base(configProvider)
    {
        // Empty.
    }

    /// <inheritdoc />
    protected override async Task HandleAsync(DiscordReminderTask task, ILogger log)
    {
        var recipient = task.Recipient;

        if (!string.IsNullOrWhiteSpace(task.RecipientName))
        {
            recipient = await this.ConfigProvider.GetNamedDiscordRecipientAsync(task.RecipientName, log);

            if (recipient is null)
            {
                throw new Exception($"Named recipient {task.RecipientName} not found.");
            }
        }

        // TODO: Implement calling Discord APIs.
        log.LogInformation("[HANDLING DISCORD REMINDER TASK]");
    }
}
