using System.Text.Json;

namespace HarmonyBadger;

/// <summary>
/// A collection of constants used throughout the application.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The name of the directory where config files for scheduled tasks are stored.
    /// </summary>
    public const string TaskConfigsDirectoryName = "TaskConfigs";

    /// <summary>
    /// The extension used to identify scheduled task configuration files.
    /// </summary>
    public const string ScheduledTaskConfigFileExtension = ".schedule.json";

    /// <summary>
    /// The maximum number of times a schedule can be triggered during a single
    /// invocation of the scheduler function.
    /// </summary>
    public const int MaxTriggersPerSchedule = 4;

    /// <summary>
    /// The name of the Azure queue used to schedule tasks for execution.
    /// </summary>
    public const string TaskQueueName = "task-queue";

    /// <summary>
    /// Default settings to use for the .Net JSON serializer.
    /// </summary>
    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// A collection of names of metrics logged by the application.
    /// </summary>
    public static class MetricNames
    {
        /// <summary>
        /// A metric logged when loading a schedule config fails.
        /// </summary>
        public const string LoadScheduleConfigFailed = nameof(LoadScheduleConfigFailed);

        /// <summary>
        /// A metric logged when enqueuing a triggered task fails.
        /// </summary>
        public const string EnqueueTaskFailed = nameof(EnqueueTaskFailed);
    }
}
