using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels.Tasks;

/// <summary>
/// A test task that should display a configured debug message when executed.
/// </summary>
public class SendSmsTask : ITask, IValidatableObject, IJsonOnDeserialized
{
    /// <inheritdoc />
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskKind TaskKind => TaskKind.SendSms;

    /// <summary>
    /// The phone number to send the SMS message to.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// The message to send via SMS.
    /// </summary>
    public string Message { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (string.IsNullOrWhiteSpace(this.PhoneNumber))
        {
            yield return new ValidationResult(
                $"SendSms task is missing field '{nameof(this.PhoneNumber)}'");
        }

        if (!IsValidPhoneNumber(this.PhoneNumber))
        {
            yield return new ValidationResult(
                $"Phone number '{this.PhoneNumber}' is not valid. (Remove any spaces or special characters -- only include digits, and make sure it's prefixed with county code '1').");
        }

        if (this.PhoneNumber[0] != '1')
        {
            yield return new ValidationResult(
                $"Phone number '{this.PhoneNumber}' is not allowed. Only US/Canadian phone numbers are valid (and must be prefixed by country code '1').");
        }

        if (string.IsNullOrWhiteSpace(this.Message))
        {
            yield return new ValidationResult(
                $"SendSms task is missing field '{nameof(this.Message)}'");
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();

    // A fairly draconian test that does not allow for special characters or spaces. Just 11 digit phone number ('1' + 10 digit number).
    private static bool IsValidPhoneNumber(string phoneNumber) => phoneNumber?.Length == 11 && phoneNumber.All(c => char.IsDigit(c));
}
