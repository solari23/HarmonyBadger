using TimeZoneConverter;

namespace HarmonyBadgerFunctionApp;

/// <summary>
/// A collection of helpers for dealing with time.
/// </summary>
public static class TimeHelper
{
    /// <summary>
    /// The default IANA standard timezone used for evaluating schedules in this app.
    /// </summary>
    public const string DefaultLocalIanaTimeZoneName = "US/Pacific";

    /// <summary>
    /// The <see cref="TimeZoneInfo"/> corresponding to <see cref="DefaultLocalIanaTimeZoneName"/>.
    /// </summary>
    public static TimeZoneInfo DefaultLocalTimeZoneInfo
        = TZConvert.GetTimeZoneInfo(DefaultLocalIanaTimeZoneName);

    /// <summary>
    /// Converts a UTC <see cref="DateTimeOffset"/> to the default local time
    /// specified by <see cref="DefaultLocalIanaTimeZoneName"/>.
    /// </summary>
    /// <param name="utcTime">The UTC time to convert.</param>
    /// <returns>The local time.</returns>
    public static DateTimeOffset ConvertToLocal(DateTimeOffset utcTime)
        => TimeZoneInfo.ConvertTime(utcTime, DefaultLocalTimeZoneInfo);

    /// <summary>
    /// Converts a local <see cref="DateTime"/> to UTC.
    /// </summary>
    /// <param name="localTime">The local time to convert.</param>
    /// <returns>The UTC time.</returns>
    public static DateTime ConvertToUtc(DateTime localTime)
        => TimeZoneInfo.ConvertTimeToUtc(localTime, DefaultLocalTimeZoneInfo);

    /// <summary>
    /// Gets the current local time. The local timezone is defined by <see cref="DefaultLocalIanaTimeZoneName"/>.
    /// </summary>
    public static DateTimeOffset CurrentLocalTime => ConvertToLocal(DateTimeOffset.UtcNow);
}
