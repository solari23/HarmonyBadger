using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HarmonyBadgerFunctionApp.TaskModel;

[JsonConverter(typeof(SchedulePolymorphicJsonConverter))]
public interface ISchedule
{
    ScheduleKind ScheduleKind { get; }

    IEnumerable<string> ToCronExpressions(DateTime? now = null);
}
