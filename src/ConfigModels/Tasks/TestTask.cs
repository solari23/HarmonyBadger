using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels.Tasks;

/// <summary>
/// A test task that should display a configured debug message when executed.
/// </summary>
public class TestTask : ITask, IValidatableObject, IJsonOnDeserialized, ITemplatedMessage
{
    /// <inheritdoc />
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskKind TaskKind => TaskKind.Test;

    /// <inheritdoc />
    public string Message { get; set; }

    /// <inheritdoc />
    public string TemplateFilePath { get; set; }

    /// <inheritdoc />
    public Dictionary<string, string> TemplateParameters { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        foreach (var result in ((ITemplatedMessage)this).ValidateTemplatedMessageFields("Test Task"))
        {
            yield return result;
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}
