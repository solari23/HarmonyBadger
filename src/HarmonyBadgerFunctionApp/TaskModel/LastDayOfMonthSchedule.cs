using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HarmonyBadgerFunctionApp.TaskModel;

public class LastDayOfMonthSchedule : ISchedule
{
    /// <inheritdoc />
    public ScheduleKind ScheduleKind => ScheduleKind.LastDayOfMonth;

    [JsonConverter(typeof(TimeOnlyJsonConverter))]
    public TimeOnly Time { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> ToCronExpressions(DateTime? now = null)
    {
        now ??= TimeHelper.CurrentLocalTime;

        yield return $"{this.Time.Minute} {this.Time.Hour} 30 4,6,9,11 *";
        yield return $"{this.Time.Minute} {this.Time.Hour} 31 1,3,5,7,8,10,12 *";

        if (DateTime.IsLeapYear(now.Value.Year))
        {
            yield return $"{this.Time.Minute} {this.Time.Hour} 29 2 *";
        }
        else
        {
            yield return $"{this.Time.Minute} {this.Time.Hour} 28 2 *";
        }
    }
}
