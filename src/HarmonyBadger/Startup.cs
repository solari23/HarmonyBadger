using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using HarmonyBadger.Scheduler;
using HarmonyBadger.TaskProcessor.TaskHandlers;

// Register the Startup type to prepare the DI container.
[assembly: FunctionsStartup(typeof(HarmonyBadger.Startup))]

namespace HarmonyBadger;

/// <summary>
/// A startup class that prepares dependency injection for the app.
/// </summary>
public class Startup : FunctionsStartup
{
    /// <inheritdoc />
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddTransient<IScheduledTaskConfigLoader, ScheduledTaskConfigLoader>();
        builder.Services.AddTransient<IClock, Clock>();
        builder.Services.AddTransient<ITaskHandlerFactory, TaskHandlerFactory>();
    }
}
