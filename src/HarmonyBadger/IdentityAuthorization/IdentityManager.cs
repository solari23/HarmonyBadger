using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace HarmonyBadger.IdentityAuthorization;

/// <summary>
/// Service that handles interop with Identity Providers.
/// </summary>
public interface IIdentityManager
{
    /// <summary>
    /// Validates the given ID Token and checks that it corresponds to an authorized user.
    /// </summary>
    /// <returns>A result that either contains the email address of the authorized user, or an error.</returns>
    Task<Result<string>> ValidateIsAuthorizedUserAsync(string idToken);
}

public class IdentityManager : IIdentityManager
{
    public IdentityManager(IMemoryCache memoryCache, IConfiguration appSettings)
    {
        this.MemoryCache = memoryCache;
        this.AppSettings = appSettings;
    }

    private IMemoryCache MemoryCache { get; }

    private IConfiguration AppSettings { get; }

    /// <inheritdoc />
    public async Task<Result<string>> ValidateIsAuthorizedUserAsync(string idToken)
    {
        var oidcConfig = await this.MemoryCache.GetOrCreateAsync(
            "MSIdentityOidcConfig",
            cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return GetMSIdentityOidcConfigAsync();
            });

        var validationParameters = new TokenValidationParameters
        {
            ValidAudience = this.AppSettings.MSIdentityAppId(),
            ValidIssuers = this.AppSettings.MSIdentityIssuers(),
            IssuerSigningKeys = oidcConfig.SigningKeys,
        };

        var tokenValidator = new JwtSecurityTokenHandler();
        var tokenValidationResult = await tokenValidator.ValidateTokenAsync(idToken, validationParameters);

        if (!tokenValidationResult.IsValid)
        {
            return Result<string>.FromError("id_token validation failed", tokenValidationResult.Exception);
        }

        if (!tokenValidationResult.Claims.ContainsKey(ClaimTypes.Email)
            || tokenValidationResult.Claims[ClaimTypes.Email] is not string emailClaim)
        {
            return Result<string>.FromError("id_token does not contain email claim");
        }

        if (!await this.AppSettings.IsAuthorizedUserAsync(emailClaim))
        {
            return Result<string>.FromError($"User '{emailClaim}' is not authorized.");
        }

        return Result<string>.Success(emailClaim);
    }

    private static async Task<OpenIdConnectConfiguration> GetMSIdentityOidcConfigAsync()
    {
        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());
        var config =  await configManager.GetConfigurationAsync();
        return config;
    }
}
