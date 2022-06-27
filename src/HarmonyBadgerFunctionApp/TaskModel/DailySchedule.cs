using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HarmonyBadgerFunctionApp.TaskModel;

public class DailySchedule : ISchedule
{
    /// <inheritdoc />
    public ScheduleKind ScheduleKind => ScheduleKind.Daily;

    [JsonConverter(typeof(TimeOnlyJsonConverter))]
    public TimeOnly Time { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> ToCronExpressions(DateTime? _ = null)
    {
        yield return $"{this.Time.Minute} {this.Time.Hour} * * *";
    }
}
