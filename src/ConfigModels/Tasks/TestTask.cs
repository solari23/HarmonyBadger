using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels.Tasks;

/// <summary>
/// A test task that should display a configured debug message when executed.
/// </summary>
public class TestTask : ITask, IValidatableObject, IJsonOnDeserialized
{
    /// <inheritdoc />
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskKind TaskKind => TaskKind.Test;

    /// <summary>
    /// A debug message to display.
    /// </summary>
    public string DebugMessage { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (string.IsNullOrEmpty(this.DebugMessage))
        {
            yield return new ValidationResult(
                $"Test task is missing field '{nameof(this.DebugMessage)}'");
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}
