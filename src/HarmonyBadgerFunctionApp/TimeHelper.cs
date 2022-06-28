using System;
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
    public static TimeZoneInfo DefaultTimeZoneInfo
        = TZConvert.GetTimeZoneInfo(DefaultLocalIanaTimeZoneName);

    /// <summary>
    /// Gets the current local time. The local timezone is defined by <see cref="DefaultLocalIanaTimeZoneName"/>.
    /// </summary>
    public static DateTime CurrentLocalTime
        => TimeZoneInfo.ConvertTime(DateTime.UtcNow, DefaultTimeZoneInfo);
}
