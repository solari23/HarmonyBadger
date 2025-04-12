using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels.Schedules;

/// <summary>
/// A schedule set to occur on the same day every month at a fixed time.
///
/// If the month does not contain that day (e.g. "February 30"), the schedule
/// instead triggers on the last day of the month.
/// </summary>
public class MonthlySchedule : ISchedule, IValidatableObject, IJsonOnDeserialized
{
    /// <inheritdoc />
    public ScheduleKind ScheduleKind => ScheduleKind.Monthly;

    /// <summary>
    /// The day of the month for the schedule to trigger (1-31).
    /// </summary>
    public int? DayOfMonth { get; set; }

    /// <summary>
    /// The scheduled time of day.
    /// </summary>
    [JsonConverter(typeof(TimeOnlyJsonConverter))]
    public TimeOnly? Time { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> ToCronExpressions(DateTime? now = null)
    {
        now ??= DateTime.UtcNow;
        var dayOfMonth = this.DayOfMonth.Value;
        var time = this.Time.Value;

        if (dayOfMonth == 31)
        {
            // DayOfMonth = 31 reduces to "the last day of the month".
            // Re-use logic from the LastDayOfMonthSchedule.
            var schedule = new LastDayOfMonthSchedule
            {
                Time = time,
            };

            foreach (var cron in schedule.ToCronExpressions(now))
            {
                yield return cron;
            }
        }
        else if (dayOfMonth > 28)
        {
            // For 29 or 30 days, we need special handling for February.
            if (DateTime.IsLeapYear(now.Value.Year))
            {
                yield return $"{time.Minute} {time.Hour} 29 2 *";
            }
            else
            {
                yield return $"{time.Minute} {time.Hour} 28 2 *";
            }

            // All other months have at least the required number of days.
            yield return $"{time.Minute} {time.Hour} {dayOfMonth} 1,3,4,5,6,7,8,9,10,11,12 *";
        }
        else
        {
            // Every month has at least 28 days.
            yield return $"{time.Minute} {time.Hour} {dayOfMonth} * *";
        }
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (this.DayOfMonth is null)
        {
            yield return new ValidationResult(
                $"Monthly schedule is missing field '{nameof(this.DayOfMonth)}'");
        }
        else if (this.DayOfMonth < 1 || this.DayOfMonth > 31)
        {
            yield return new ValidationResult(
                $"Monthly schedule has '{nameof(this.DayOfMonth)}' set to {this.DayOfMonth}. Must be between [1-31].");
        }

        if (this.Time is null)
        {
            yield return new ValidationResult(
                $"Monthly schedule is missing field '{nameof(this.Time)}'");
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}
