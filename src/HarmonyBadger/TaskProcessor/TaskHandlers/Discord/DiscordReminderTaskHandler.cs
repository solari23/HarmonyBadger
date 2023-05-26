using Discord;
using Discord.Rest;
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
    public DiscordReminderTaskHandler(IConfigProvider configProvider)
    {
        this.ConfigProvider = configProvider;
    }

    private IConfigProvider ConfigProvider { get; }

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

        var botSecret = Environment.GetEnvironmentVariable(Constants.SecretEnvVarNames.DiscordBotSecretKey);

        // It's a bit wasteful to create a new client and authenticate on every task.
        // A better implementation would be to use a pool of clients that are cached and recreated
        // after a certain period of time (to guard against token expiry).
        // This works for now since the volume on HarmonyBadger is extremely low.
        using var client = new DiscordRestClient();
        await client.LoginAsync(TokenType.Bot, botSecret);

        // Get the channel to send the message to.
        var guild = await client.GetGuildAsync(recipient.GuildId.Value);

        // Discord will return an error if the guild doesn't exist or if the
        // bot doesn't have authZ, and the library will throw the error.
        // But, just for completeness, we'll null check.
        if (guild is null)
        {
            throw new Exception($"Guild '{recipient.GuildId.Value}' was not found.");
        }

        var channel = await guild.GetTextChannelAsync(recipient.ChannelId.Value);

        // Same as for guild case above, if the channel doesn't exist we likely
        // won't make it here.
        if (channel is null)
        {
            throw new Exception($"Guild '{recipient.ChannelId.Value}' was not found.");
        }

        await channel.SendMessageAsync(task.Message);
        log.LogInformation($"Delivered reminder message to channel {channel.Name}");
    }
}
