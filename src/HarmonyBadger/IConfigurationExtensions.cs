using Microsoft.Extensions.Configuration;

namespace HarmonyBadger;

/// <summary>
/// Helpers to access config values in appsettings.json files.
/// </summary>
public static class IConfigurationExtensions
{
    private static readonly SemaphoreSlim configReadLock = new SemaphoreSlim(1);

    public static string MSIdentityAppId(this IConfiguration appSettings)
        => appSettings.GetValue<string>("MSIdentityAppId");

    public static string MSIdentityOidcConfigUrl(this IConfiguration appSettings)
        => appSettings.GetValue<string>("MSIdentityOidcConfigUrl");

    public static string[] MSIdentityIssuers(this IConfiguration appSettings)
        => appSettings.GetSection("MSIdentityIssuers").Get<string[]>();

    private static HashSet<string> authorizedUsers;

    public static async Task<bool> IsAuthorizedUserAsync(this IConfiguration appSettings, string email)
    {
        if (authorizedUsers is null)
        {
            try
            {
                await configReadLock.WaitAsync();
                if (authorizedUsers is null)
                {
                    authorizedUsers = new HashSet<string>(
                        appSettings.GetSection("AuthorizedUsers").Get<string[]>() ?? Array.Empty<string>(),
                        StringComparer.OrdinalIgnoreCase);
                }
            }
            finally
            {
                configReadLock.Release();
            }
        }

        return authorizedUsers.Contains(email);
    }
}
