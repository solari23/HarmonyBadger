using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace HarmonyBadger.ConfigModels.Tasks;

/// <summary>
/// A task that sends an email.
/// </summary>
public class SendEmailTask : ITask, IValidatableObject, IJsonOnDeserialized, ITemplatedMessage
{
    /// <inheritdoc />
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskKind TaskKind => TaskKind.SendEmail;

    /// <summary>
    /// The email address of the message sender. If not specified, the default
    /// sender as specied in config value "DefaultEmailSendAccount" will be used.
    ///
    /// This user must have authorized HarmonyBadger to send email on their behalf
    /// via the authorization endpoint.
    /// </summary>
    public string Sender { get; set; }

    /// <summary>
    /// The email addresses of message recipients on the 'to' line.
    /// </summary>
    public string[] ToRecipients { get; set; }

    /// <summary>
    /// The email addresses of message recipients on the 'CC' line.
    /// </summary>
    public string[] CCRecipients { get; set; }

    /// <summary>
    /// The email addresses of message recipients to be BCCed (blind carbon copied).
    /// </summary>
    public string[] BccRecipients { get; set; }

    /// <summary>
    /// The subject of the message.
    /// </summary>
    public string Subject { get; set; }

    /// <inheritdoc />
    public string Message { get; set; }

    /// <inheritdoc />
    public string TemplateFilePath { get; set; }

    /// <inheritdoc />
    public Dictionary<string, string> TemplateParameters { get; set; }

    /// <summary>
    /// Whether or not to flag the message (Default: false).
    /// </summary>
    public bool HighImportance { get; set; } = false;

    /// <summary>
    /// Indicates whether or not to treat the email body message as HTML (Default: false).
    /// </summary>
    /// <remarks>
    /// Using a <see cref="TemplateFilePath"/> that ends with extension ".html" will
    /// automatically be treated as HTML regarless of this setting.
    /// </remarks>
    public bool IsHtml { get; set; } = false;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (this.Sender is not null && !IsValidEmail(this.Sender))
        {
            yield return new ValidationResult(
                $"Field '{nameof(this.Sender)}' in SendEmail task must be a valid email address (if specified)");
        }

        if (this.ToRecipients is null || this.ToRecipients.Length == 0)
        {
            yield return new ValidationResult(
                $"SendEmail task must have a non-empty list of '{nameof(this.ToRecipients)}'");
        }
        else
        {
            for (int i = 0; i < this.ToRecipients.Length; i++)
            {
                if (!IsValidEmail(this.ToRecipients[i]))
                {
                    yield return new ValidationResult(
                        $"Recipient in field '{nameof(this.ToRecipients)}' at index {i} is not a valid email address");
                }
            }
        }

        if (this.CCRecipients is not null)
        {
            for (int i = 0; i < this.CCRecipients.Length; i++)
            {
                if (!IsValidEmail(this.CCRecipients[i]))
                {
                    yield return new ValidationResult(
                        $"Recipient in field '{nameof(this.CCRecipients)}' at index {i} is not a valid email address");
                }
            }
        }

        if (this.BccRecipients is not null)
        {
            for (int i = 0; i < this.BccRecipients.Length; i++)
            {
                if (!IsValidEmail(this.BccRecipients[i]))
                {
                    yield return new ValidationResult(
                        $"Recipient in field '{nameof(this.BccRecipients)}' at index {i} is not a valid email address");
                }
            }
        }

        if (string.IsNullOrEmpty(this.Subject))
        {
            yield return new ValidationResult(
                $"SendEmail task is missing field '{nameof(this.Subject)}'");
        }

        foreach (var result in ((ITemplatedMessage)this).ValidateTemplatedMessageFields("SendEmail task"))
        {
            yield return result;
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();

    private static bool IsValidEmail(string email)
    {
        const string EmailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
        return Regex.IsMatch(email, EmailPattern, RegexOptions.Compiled, matchTimeout: TimeSpan.FromMilliseconds(50));
    }
}
