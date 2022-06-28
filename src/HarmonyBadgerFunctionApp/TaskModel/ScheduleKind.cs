namespace HarmonyBadgerFunctionApp.TaskModel;

/// <summary>
/// Identifies the mechanism by which a schedule is defined.
/// </summary>
public enum ScheduleKind
{
    Invalid = 0,

    /// <summary>
    /// A schedule based on a CRON expression.
    /// See <see cref="CronSchedule"/> for more details.
    /// </summary>
    Cron = 1,

    /// <summary>
    /// A schedule set to occur once per day at a fixed time.
    /// See <see cref="DailySchedule"/> for more details.
    /// </summary>
    Daily = 2,

    /// <summary>
    /// A schedule set to occur once per week on a fixed day at a fixed time.
    /// See <see cref="WeeklySchedule"/> for more details.
    /// </summary>
    Weekly = 3,

    /// <summary>
    /// A schedule set to occur on the last day of the month at a fixed time.
    /// See <see cref="LastDayOfMonthSchedule"/> for more details.
    /// </summary>
    LastDayOfMonth = 4,
}
