namespace HarmonyBadgerFunctionApp;

/// <summary>
/// A collection of constants used throughout the project.
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
}
