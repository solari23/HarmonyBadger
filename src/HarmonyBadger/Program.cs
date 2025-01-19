using System.Text.Json;

using HarmonyBadger.IdentityAuthorization;
using HarmonyBadger.TaskProcessor.TaskHandlers;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HarmonyBadger;

/// <summary>
/// Contains the entry point for the application.
/// </summary>
public static class Program
{
    /// <summary>
    /// The entry point for the application.
    /// </summary>
    public static void Main()
    {
#if DEBUG
        LoadUserSecretsToEnvironmentVariables();
#endif

        var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureAppConfiguration(ConfigureAppConfiguration)
            .ConfigureServices(ConfigureServices)
            .ConfigureLogging(ConfigureLogging)
            .Build();

        host.Run();
    }

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
    {
        var hostEnvironment = context.HostingEnvironment;
        builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddAzureClients(configureClients =>
        {
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            configureClients.AddQueueServiceClient(storageConnectionString).WithName(Constants.DefaultStorageClientName);
            configureClients.AddTableServiceClient(storageConnectionString).WithName(Constants.DefaultStorageClientName);
        });

        services.AddTransient<IClock, Clock>();
        services.AddScoped<IConfigProvider, ConfigProvider>();
        services.AddTransient<ITaskHandlerFactory, TaskHandlerFactory>();
        services.AddSingleton<IIdentityManager, IdentityManager>();
        services.AddSingleton<ITokenStorage, TokenStorage>();
        services.AddSingleton<IEmailClient, MSGraphEmailClient>();
        services.AddSingleton<ITemplateEngine, DotLiquidTemplateEngine>();
        services.AddMemoryCache();
    }

    private static void ConfigureLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.Services.Configure<LoggerFilterOptions>(options =>
        {
            // Removes a default rule set up by AppInsights that filters out logs below 'warning'.
            // https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=hostbuilder%2Cwindows#managing-log-levels
            LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    }

#if DEBUG
    /// <summary>
    /// Normally UserSecrets work with IConfiguration, but with Azure Functions it's
    /// a cleaner integration to have them available in environment variables so
    /// that they are in line with App Settings.
    ///
    /// This function will access the UserSecrets on local disk and push them to
    /// environment variables.
    /// </summary>
    private static void LoadUserSecretsToEnvironmentVariables()
    {
        var userSecretsIdAttribute = typeof(Program).Assembly
            .GetCustomAttributes(typeof(UserSecretsIdAttribute), inherit: false)
            .FirstOrDefault() as UserSecretsIdAttribute;

        if (userSecretsIdAttribute is not null)
        {
            var userSecretsFilePath = PathHelper.GetSecretsPathFromSecretsId(
                userSecretsIdAttribute.UserSecretsId);

            if (File.Exists(userSecretsFilePath))
            {
                var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    File.OpenRead(userSecretsFilePath));

                foreach (var secret in secrets)
                {
                    Environment.SetEnvironmentVariable(secret.Key, secret.Value);
                }
            }
        }
    }
#endif
}
