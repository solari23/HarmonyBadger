using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels.Schedules;

/// <summary>
/// A schedule set to occur on a fixed date at a fixed time.
/// </summary>
public class FixedDateSchedule : ISchedule, IValidatableObject, IJsonOnDeserialized
{
    /// <inheritdoc />
    public ScheduleKind ScheduleKind => ScheduleKind.FixedDate;

    /// <summary>
    /// The date for the schedule to trigger.
    /// </summary>
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateOnly? Date { get; set; }

    /// <summary>
    /// The scheduled time of day.
    /// </summary>
    [JsonConverter(typeof(TimeOnlyJsonConverter))]
    public TimeOnly? Time { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> ToCronExpressions(DateTime? now = null)
    {
        now ??= DateTime.UtcNow;
        var date = this.Date.Value;
        var time = this.Time.Value;

        // Our CRON expression library doesn't allow specifying a year.
        // Only return a CRON expression if the current time matches the year
        // that the schedule is set for.
        if (now.Value.Year == date.Year)
        {
            yield return $"{time.Minute} {time.Hour} {date.Day} {date.Month} *";
        }
        else
        {
            yield return null;
        }
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (this.Date is null)
        {
            yield return new ValidationResult(
                $"FixedDate schedule is missing field '{nameof(this.Date)}'");
        }

        if (this.Time is null)
        {
            yield return new ValidationResult(
                $"FixedDate schedule is missing field '{nameof(this.Time)}'");
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}