using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HarmonyBadgerFunctionApp.TaskModel;

/// <summary>
/// A schedule set to occur once per day at a fixed time.
/// </summary>
public class DailySchedule : ISchedule
{
    /// <inheritdoc />
    public ScheduleKind ScheduleKind => ScheduleKind.Daily;

    /// <summary>
    /// The scheduled time of day.
    /// </summary>
    [JsonConverter(typeof(TimeOnlyJsonConverter))]
    public TimeOnly Time { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> ToCronExpressions(DateTime? _ = null)
    {
        yield return $"{this.Time.Minute} {this.Time.Hour} * * *";
    }
}
