using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels;

/// <summary>
/// The interface for all schedule mechanism implementations.
/// </summary>
[JsonConverter(typeof(SchedulePolymorphicJsonConverter))]
public interface ISchedule
{
    /// <summary>
    /// The identifier for the schedule mechanism.
    /// </summary>
    ScheduleKind ScheduleKind { get; }

    /// <summary>
    /// Converts the schedule to its equivalent CRON expression(s), which
    /// are used at runtime to evaluate if the schedule triggers.
    /// This is a returned as collection because some schedule semantics cannot
    /// be defined with a single CRON expression (e.g. <see cref="LastDayOfMonthSchedule"/>).
    /// </summary>
    /// <param name="now">
    /// Allows overriding the current time for testing scenarios. Defaults to the current time if unspecified.
    /// </param>
    /// <returns>The list of CRON expressions which are semantically equivalent to the schedule.</returns>
    IEnumerable<string> ToCronExpressions(DateTime? now = null);
}
