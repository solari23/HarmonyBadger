using System.ComponentModel.DataAnnotations;

namespace HarmonyBadger.ConfigModels;

/// <summary>
/// An interface for an object that contains a templated message, along with parameters to fill the template.
/// The message can either be contained in the object as a string, or the object contains a path to a template file.
/// </summary>
public interface ITemplatedMessage
{
    /// <summary>
    /// The message, potentially with templated formatting.
    /// </summary>
    /// <remarks>
    /// Either <see cref="Message"/> or <see cref="TemplateFilePath"/> is required.
    /// </remarks>
    string Message { get; }

    /// <summary>
    /// A path to a template file.
    /// </summary>
    /// <remarks>
    /// Either <see cref="Message"/> or <see cref="TemplateFilePath"/> is required.
    /// </remarks>
    string TemplateFilePath { get; }

    /// <summary>
    /// The parameters (aka variable values) to use when filling templates.
    /// </summary>
    Dictionary<string, string> TemplateParameters { get; }

    /// <summary>
    /// Determines whether the values provided for the parameters are valid.
    /// </summary>
    public IEnumerable<ValidationResult> ValidateTemplatedMessageFields(string objectName = "The object")
    {
        if (string.IsNullOrEmpty(this.Message) == string.IsNullOrEmpty(this.TemplateFilePath))
        {
            yield return new ValidationResult(
                $"{objectName} must specify exactly one of '{nameof(this.Message)}' or '{nameof(this.TemplateFilePath)}'");
        }
    }
}
