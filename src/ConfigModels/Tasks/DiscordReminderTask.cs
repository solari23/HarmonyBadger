using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels.Tasks;

/// <summary>
/// A task that sends a reminder over Discord.
/// </summary>
public class DiscordReminderTask : ITask
{
    /// <inheritdoc />
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskKind TaskKind => TaskKind.DiscordReminder;
}
