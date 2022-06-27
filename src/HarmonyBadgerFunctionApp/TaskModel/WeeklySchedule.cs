using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HarmonyBadgerFunctionApp.TaskModel;

public class WeeklySchedule : ISchedule
{
    /// <inheritdoc />
    public ScheduleKind ScheduleKind => ScheduleKind.Weekly;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DayOfWeek Day { get; set; }

    [JsonConverter(typeof(TimeOnlyJsonConverter))]
    public TimeOnly Time { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> ToCronExpressions(DateTime? _ = null)
    {
        yield return $"{this.Time.Minute} {this.Time.Hour} * * {(int)this.Day}";
    }
}
