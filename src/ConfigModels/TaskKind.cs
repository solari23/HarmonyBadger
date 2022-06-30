using HarmonyBadger.ConfigModels.Tasks;

namespace HarmonyBadger.ConfigModels;

/// <summary>
/// Identifies a type of task.
/// </summary>
public enum TaskKind
{
    /// <summary>
    /// A task that prints a debug message when executed.
    /// See <see cref="TestTask"/> for more details.
    /// </summary>
    Test = 0,
}
