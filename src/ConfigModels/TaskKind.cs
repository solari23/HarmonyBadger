﻿using HarmonyBadger.ConfigModels.Tasks;

namespace HarmonyBadger.ConfigModels;

/// <summary>
/// Identifies a type of task.
/// </summary>
public enum TaskKind
{
    //=========================================================================
    // Adding a new task type:
    //   - Add the TaskKind value below
    //   - Define the ITask config structure in Tasks directory
    //     - Don't forget to mark the new task's TaskConfig property with
    //       [JsonConverter(typeof(JsonStringEnumConverter))]
    //   - Define the handler in <root>\HarmonyBadger\TaskProessor\TaskHandlers
    //     by inheriting off TaskHandlerBase<Your_New_ITask>
    //   - Update TaskHandlerFactory.CreateHandler
    //   - Update TaskPolymorphicJsonConverter.Read
    //   - Update TaskPolymorphicJsonConverter.Write
    //=========================================================================

    /// <summary>
    /// A task that prints a debug message when executed.
    /// See <see cref="TestTask"/> for more details.
    /// </summary>
    Test = 0,

    /// <summary>
    /// A task that sends a reminder over Discord.
    /// See <see cref="DiscordReminderTask"/> for more details.
    /// </summary>
    DiscordReminder = 1,

    /// <summary>
    /// An internal utility task that forces HarmonyBadger to refresh tokens in storage so that they do not expire.
    /// See <see cref="ForceRefreshTokenTask"/> for more details.
    /// </summary>
    ForceRefreshToken = 2,

    /// <summary>
    /// A task that sends an email.
    /// See <see cref="SendEmailTask"/> for more details.
    /// </summary>
    SendEmail = 3,
}
