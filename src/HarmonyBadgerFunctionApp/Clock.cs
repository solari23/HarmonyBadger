using System;

namespace HarmonyBadgerFunctionApp;

/// <summary>
/// Interface to assist in using Dependency Injection
/// to pass in the current time in a testable manner.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current time in UTC.
    /// </summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Gets the current time in the configured local timezone.
    /// </summary>
    /// <seealso cref="TimeHelper.DefaultLocalIanaTimeZoneName"/>
    DateTimeOffset LocalNow { get; }
}

/// <summary>
/// Concrete implementation of a clock that returns the actual current time.
/// </summary>
public class Clock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public DateTimeOffset LocalNow => TimeHelper.CurrentLocalTime;
}
