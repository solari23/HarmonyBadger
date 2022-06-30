using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels;

/// <summary>
/// Details about a task that can be executed.
/// </summary>
[JsonConverter(typeof(TaskPolymorphicJsonConverter))]
public interface ITask
{
    /// <summary>
    /// Identifies the type of task.
    /// </summary>
    TaskKind TaskKind { get; }
}
