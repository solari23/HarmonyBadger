using System;
using System.Collections.Generic;

namespace HarmonyBadgerFunctionApp.TaskModel;

/// <summary>
/// A schedule set using a CRON expression.
/// </summary>
/// <remarks>
/// The cron expressions are parsed using the NCrontab library.
/// See here for documentation on expression syntax:
/// https://github.com/atifaziz/NCrontab/wiki/Crontab-Expression
/// </remarks>
public class CronSchedule : ISchedule
{
    /// <inheritdoc />
    public ScheduleKind ScheduleKind => ScheduleKind.Cron;

    public string Expression { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> ToCronExpressions(DateTime? _ = null)
    {
        yield return this.Expression;
    }
}
