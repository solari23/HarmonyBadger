using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels.Schedules;

/// <summary>
/// A schedule set to occur once per week on a fixed day at a fixed time.
/// </summary>
public class WeeklySchedule : ISchedule, IValidatableObject, IJsonOnDeserialized
{
    /// <inheritdoc />
    public ScheduleKind ScheduleKind => ScheduleKind.Weekly;

    /// <summary>
    /// The scheduled day of the week.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DayOfWeek? Day { get; set; }

    /// <summary>
    /// The scheduled time of day.
    /// </summary>
    [JsonConverter(typeof(TimeOnlyJsonConverter))]
    public TimeOnly? Time { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> ToCronExpressions(DateTime? _ = null)
    {
        yield return $"{this.Time.Value.Minute} {this.Time.Value.Hour} * * {(int)this.Day}";
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (this.Day is null)
        {
            yield return new ValidationResult(
                $"Weekly schedule is missing field '{nameof(this.Day)}'");
        }

        if (this.Time is null)
        {
            yield return new ValidationResult(
                $"Weekly schedule is missing field '{nameof(this.Time)}'");
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}
