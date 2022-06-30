using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarmonyBadgerFunctionApp.TaskModel;

/// <summary>
/// A schedule set to occur on the last day of the month at a fixed time.
/// </summary>
public class LastDayOfMonthSchedule : ISchedule, IValidatableObject, IJsonOnDeserialized
{
    /// <inheritdoc />
    public ScheduleKind ScheduleKind => ScheduleKind.LastDayOfMonth;

    /// <summary>
    /// The scheduled time of day.
    /// </summary>
    [JsonConverter(typeof(TimeOnlyJsonConverter))]
    public TimeOnly? Time { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> ToCronExpressions(DateTime? now = null)
    {
        now ??= TimeHelper.CurrentLocalTime.DateTime;
        var time = this.Time.Value;

        // Handle months with 30 days.
        yield return $"{time.Minute} {time.Hour} 30 4,6,9,11 *";

        // Handle months with 31 days.
        yield return $"{time.Minute} {time.Hour} 31 1,3,5,7,8,10,12 *";

        // Handle February, taking into account leap years.
        if (DateTime.IsLeapYear(now.Value.Year))
        {
            yield return $"{time.Minute} {time.Hour} 29 2 *";
        }
        else
        {
            yield return $"{time.Minute} {time.Hour} 28 2 *";
        }
    }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (this.Time is null)
        {
            yield return new ValidationResult(
                $"LastDayOfMonth schedule is missing field '{nameof(this.Time)}'");
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}
