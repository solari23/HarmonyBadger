namespace HarmonyBadger.ConfigModels;

/// <summary>
/// Identifies the mechanism by which a schedule is defined.
/// </summary>
public enum ScheduleKind
{
    /// <summary>
    /// A schedule based on a CRON expression.
    /// See <see cref="Schedules.CronSchedule"/> for more details.
    /// </summary>
    Cron = 0,

    /// <summary>
    /// A schedule set to occur once per day at a fixed time.
    /// See <see cref="Schedules.DailySchedule"/> for more details.
    /// </summary>
    Daily,

    /// <summary>
    /// A schedule set to occur once per week on a fixed day at a fixed time.
    /// See <see cref="Schedules.WeeklySchedule"/> for more details.
    /// </summary>
    Weekly,

    /// <summary>
    /// A schedule set to occur on the same day every month.
    /// See <see cref="Schedules.MonthlySchedule"/> for more details.
    /// </summary>
    Monthly,

    /// <summary>
    /// A schedule set to occur on the last day of the month at a fixed time.
    /// See <see cref="Schedules.LastDayOfMonthSchedule"/> for more details.
    /// </summary>
    LastDayOfMonth,

    /// <summary>
    /// A schedule set to occur on a fixed date, at a fixed time.
    /// See <see cref="Schedules.FixedDateSchedule"/> for more details.
    /// </summary>
    FixedDate,
}
