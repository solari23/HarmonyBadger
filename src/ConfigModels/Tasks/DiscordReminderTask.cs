using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using HarmonyBadger.ConfigModels.Discord;

namespace HarmonyBadger.ConfigModels.Tasks;

/// <summary>
/// A task that sends a reminder over Discord.
/// </summary>
public class DiscordReminderTask : ITask, IValidatableObject, IJsonOnDeserialized, ITemplatedMessage
{
    /// <inheritdoc />
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskKind TaskKind => TaskKind.DiscordReminder;

    /// <summary>
    /// The recipient for the reminder.
    /// A must specify exactly one of either <see cref="Recipient"/> or <see cref="RecipientName"/>.
    /// </summary>
    public DiscordRecipient Recipient { get; set; }

    /// <summary>
    /// The name of a named recipient from a Discord Recipients config file.
    /// A must specify exactly one of either <see cref="Recipient"/> or <see cref="RecipientName"/>.
    /// </summary>
    public string RecipientName { get; set; }

    /// <inheritdoc />
    public string Message { get; set; }

    /// <inheritdoc />
    public string TemplateFilePath { get; set; }

    /// <inheritdoc />
    public Dictionary<string, string> TemplateParameters { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if ((this.Recipient is null && string.IsNullOrEmpty(this.RecipientName)
            || (this.Recipient is not null && !string.IsNullOrEmpty(this.RecipientName))))
        {
            yield return new ValidationResult(
                $"DiscordReminder task must specify exactly of of '{nameof(this.Recipient)}' and '{nameof(this.RecipientName)}'");
        }

        if (string.IsNullOrEmpty(this.Message))
        {
            yield return new ValidationResult(
                $"DiscordReminder task is missing field '{nameof(this.Message)}'");
        }

        foreach (var result in ((ITemplatedMessage)this).ValidateTemplatedMessageFields("DiscordReminder task"))
        {
            yield return result;
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}
