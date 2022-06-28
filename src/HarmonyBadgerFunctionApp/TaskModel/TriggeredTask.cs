using System;
using System.Security.Cryptography;
using System.Text;

namespace HarmonyBadgerFunctionApp.TaskModel;

/// <summary>
/// Details about the activation of a <see cref="ScheduledTask"/>.
/// </summary>
public class TriggeredTask
{
    /// <summary>
    /// Helper to generate the unique <see cref="TriggerId"/> based on the
    /// <see cref="ScheduledTask"/> config checksum and the trigger time.
    /// </summary>
    /// <param name="scheduleConfigChecksum">The <see cref="ScheduledTask"/> config checksum.</param>
    /// <param name="triggerTimeUtc">The time when the triggered task will execute.</param>
    /// <returns></returns>
    public static string CreateTriggerId(string scheduleConfigChecksum, DateTime triggerTimeUtc)
    {
        var stringToHash = scheduleConfigChecksum + triggerTimeUtc.ToString("s");
        var bytesToHash = Encoding.Default.GetBytes(stringToHash);

        using var hasher = SHA256.Create();
        return BitConverter.ToString(hasher.ComputeHash(bytesToHash)).Replace("-", string.Empty);
    }

    /// <summary>
    /// A deterministic unique identifier for the schedule trigger event.
    /// </summary>
    public string TriggerId { get; set; }

    /// <summary>
    /// The time when the triggered task should execute.
    /// </summary>
    public DateTime TriggerTimeUtc { get; set; }

    /// <summary>
    /// The name of the <see cref="ScheduledTask"/> configuration that
    /// the task was triggered from.
    /// </summary>
    public string ScheduleConfigName { get; set; }

    /// <summary>
    /// The checksum of the <see cref="ScheduledTask"/> configuration
    /// that the task was triggered from.
    /// </summary>
    public string ScheduleConfigChecksum { get; set; }

    /// <summary>
    /// The InvocationId of the scheduler function invocation.
    /// </summary>
    public string EvaluatingFunctionInvocationId { get; set; }

    /// <summary>
    /// The time of the schedules function invocation.
    /// </summary>
    public DateTime EvaluationTimeUtc { get; set; }
}
