﻿using HarmonyBadgerFunctionApp.Scheduler;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// Register the Startup type to prepare the DI container.
[assembly: FunctionsStartup(typeof(HarmonyBadgerFunctionApp.Startup))]

namespace HarmonyBadgerFunctionApp;

/// <summary>
/// A startup class that prepares dependency injection for the app.
/// </summary>
public class Startup : FunctionsStartup
{
    /// <inheritdoc />
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddTransient<IScheduledTaskConfigLoader, ScheduledTaskConfigLoader>();
    }
}