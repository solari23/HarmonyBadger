using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels.Discord;

/// <summary>
/// Encapsulates information about an entity that can receive messages via Discord.
/// </summary>
public class DiscordRecipient : IValidatableObject, IJsonOnDeserialized
{
    /// <summary>
    /// If a named recipient, gets the name of the file that that the
    /// <see cref="DiscordRecipient"/> configuration was loaded from.
    /// </summary>
    [JsonIgnore]
    public string ConfigFileName { get; set; }

    /// <summary>
    /// The GuildId (aka server) to deliever the message to.
    /// </summary>
    public long? GuildId { get; set; }

    /// <summary>
    /// The text channel to deliver the message to.
    /// </summary>
    public long? ChannelId { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (this.GuildId is null)
        {
            yield return new ValidationResult(
                $"Discord recipient is missing required field '{nameof(this.GuildId)}'");
        }

        if (this.ChannelId is null)
        {
            yield return new ValidationResult(
                $"Discord recipient is missing required field '{nameof(this.ChannelId)}'");
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}
