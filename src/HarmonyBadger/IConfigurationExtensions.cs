using Microsoft.Extensions.Configuration;

namespace HarmonyBadger;

/// <summary>
/// Helpers to access config values in appsettings.json files.
/// </summary>
public static class IConfigurationExtensions
{
    public static string MSIdentityAppId(this IConfiguration appSettings)
        => appSettings.GetValue<string>("MSIdentityAppId");

}
