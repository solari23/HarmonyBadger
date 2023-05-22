using HarmonyBadger.ConfigModels;
using HarmonyBadger.IdentityAuthorization;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// A factory that provides the appropriate <see cref="ITaskHandler"/> for
/// the desired <see cref="TaskKind"/>.
/// </summary>
public interface ITaskHandlerFactory
{
    /// <summary>
    /// Creates and returns a <see cref="ITaskHandler"/> that can handle
    /// <see cref="ITask"/> instances of the given <see cref="TaskKind"/>.
    /// </summary>
    /// <param name="taskKind">The kind of task to get a handler for.</param>
    /// <returns>The specialized type handler.</returns>
    ITaskHandler CreateHandler(TaskKind taskKind);
}

/// <inheritdoc/>
public class TaskHandlerFactory : ITaskHandlerFactory
{
    /// <summary>
    /// Creates a new instance of the <see cref="TaskHandlerFactory"/> class.
    /// </summary>
    public TaskHandlerFactory(IConfigProvider configProvider, IIdentityManager identityManager)
    {
        this.ConfigProvider = configProvider;
        this.IdentityManager = identityManager;
    }

    private IConfigProvider ConfigProvider { get; }

    private IIdentityManager IdentityManager { get; }

    /// <inheritdoc />
    public ITaskHandler CreateHandler(TaskKind taskKind) => taskKind switch
    {
        TaskKind.Test => new TestTaskHandler(this.ConfigProvider),
        TaskKind.DiscordReminder => new DiscordReminderTaskHandler(this.ConfigProvider),
        TaskKind.ForceRefreshToken => new ForceRefreshTokenTaskHandler(this.ConfigProvider, this.IdentityManager),
        _ => throw new NotImplementedException($"No handler is defined for {taskKind} tasks."),
    };
}
