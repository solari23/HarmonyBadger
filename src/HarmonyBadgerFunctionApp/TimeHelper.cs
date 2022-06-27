using System;
using TimeZoneConverter;

namespace HarmonyBadgerFunctionApp;

public static class TimeHelper
{
    public const string DefaultIanaTimeZoneName = "US/Pacific";

    public static TimeZoneInfo DefaultTimeZoneInfo
        = TZConvert.GetTimeZoneInfo(DefaultIanaTimeZoneName);

    public static DateTime CurrentLocalTime
        => TimeZoneInfo.ConvertTime(DateTime.UtcNow, DefaultTimeZoneInfo);
}
