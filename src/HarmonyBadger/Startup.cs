using System.Text.Json;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;

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
        builder.Services.AddTransient<IClock, Clock>();
        builder.Services.AddScoped<IConfigProvider, ConfigProvider>();
        builder.Services.AddTransient<ITaskHandlerFactory, TaskHandlerFactory>();

#if DEBUG
        LoadUserSecretsToEnvironmentVariables();
#endif
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
        var userSecretsIdAttribute = typeof(Startup).Assembly
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
