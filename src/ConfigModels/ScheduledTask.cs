using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels;

/// <summary>
/// The schema for a task that needs to be executed on a schedule.
///
/// The tasks are configured as JSON documents read from the directory
/// specified in constant <see cref="Constants.TaskConfigsDirectoryName"/>.
/// </summary>
public class ScheduledTask : IValidatableObject, IJsonOnDeserialized
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

    /// <summary>
    /// Details about the task to run.
    /// </summary>
    public ITask Task { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (this.Schedule is null || this.Schedule.Count == 0)
        {
            yield return new ValidationResult(
                $"Schedule config does not define schedules in field '{nameof(this.Schedule)}'");
        }

        if (this.Task is null)
        {
            yield return new ValidationResult(
                $"Schedule missing details about task to run in field '{nameof(this.Schedule)}'");
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}
