using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HarmonyBadgerFunctionApp.TaskModel;

/// <summary>
/// The schema for a task that needs to be executed on a schedule.
///
/// The tasks are configured as JSON documents read from the directory
/// specified in constant <see cref="Constants.TaskConfigsDirectoryName"/>.
/// </summary>
public class ScheduledTask
{
    /// <summary>
    /// Gets the name of the file that that the <see cref="ScheduledTask"/>
    /// configuration was loaded from.
    /// </summary>
    [JsonIgnore]
    public string ConfigFileName { get; set; }

    /// <summary>
    /// Gets the SHA256 checksum of the file that the <see cref="ScheduledTask"/>
    /// configuration was loaded from.
    /// </summary>
    [JsonIgnore]
    public string Checksum { get; set; }

    /// <summary>
    /// Indicates whether or not the task is currently enabled.
    /// Disabled tasks will not be executed by the system.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// The schedule(s) that define what time the task should be run.
    /// </summary>
    public List<ISchedule> Schedule { get; set; }
}
