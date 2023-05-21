using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HarmonyBadger.IdentityAuthorization;

/// <summary>
/// Service that handles interop with Identity Providers.
/// </summary>
public interface IIdentityManager
{
    /// <summary>
    /// Gets the IdP URI to send the user to so that they can authorize HarmonyBadger.
    ///
    /// Today, this integrates only with Microsoft Identity Platform, but this can be extended
    /// to work with the OAuth IdPs of any 3rd party services that HarmonyBadger will integrate with.
    /// </summary>
    /// <param name="scopes">The authorization scopes to request.</param>
    /// <returns>The URI to the IdP's authorize endpoint, with full OAuth parameters.</returns>
    Uri GetAuthorizationRequestUri(IEnumerable<string> scopes);

    /// <summary>
    /// Validates the given ID Token and checks that it corresponds to an authorized user.
    /// </summary>
    /// <returns>A result that either contains the email address of the authorized user, or an error.</returns>
    Task<Result<string>> ValidateIsAuthorizedUserAsync(string idToken);

    /// <summary>
    /// Redeems the given authorization code and stores the resulting refresh token.
    /// </summary>
    /// <param name="authCode">The authorization code to redeem.</param>
    /// <returns>A result which of 'true' if the operation succeeded, or diagnostic error information if not.</returns>
    Task<Result<bool>> RedeemAuthCodeAndSaveRefreshTokenAsync(string authCode);
}

public class IdentityManager : IIdentityManager
{
    public static string HarmonyBadgerRedirectUri
        => $"https://{Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}/authorization";

    public IdentityManager(IMemoryCache memoryCache, IConfiguration appSettings)
    {
        this.MemoryCache = memoryCache;
        this.AppSettings = appSettings;

        var msAppId = this.AppSettings.MSIdentityAppId();
        var msAppCert = GetMSAppCert();
        this.MSIdentityClient = new MSIdentityClient(
            msAppId,
            HarmonyBadgerRedirectUri,
            msAppCert);
    }

    private MSIdentityClient MSIdentityClient { get; }

    private IMemoryCache MemoryCache { get; }

    private IConfiguration AppSettings { get; }

    /// <inheritdoc />
    public Uri GetAuthorizationRequestUri(IEnumerable<string> scopes)
        => this.MSIdentityClient.GetAuthorizationRequestUri(scopes);

    /// <inheritdoc />
    public async Task<Result<string>> ValidateIsAuthorizedUserAsync(string idToken)
    {
        // Token validation parameters are cached since they are generated based on the IdP's OIDC metadata.
        // We don't want to hit the metadata endpoint too often.
        var tokenValidationParams = await this.MemoryCache.GetOrCreateAsync(
            "MSTokenValidationParams",
            async cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                var oidcConfig = await this.MSIdentityClient.GetOidcConfigAsync();

                return new TokenValidationParameters
                {
                    ValidAudience = this.AppSettings.MSIdentityAppId(),
                    ValidIssuers = this.AppSettings.MSIdentityIssuers(),
                    IssuerSigningKeys = oidcConfig.SigningKeys,
                };
            });

        var tokenValidator = new JwtSecurityTokenHandler();
        var tokenValidationResult = await tokenValidator.ValidateTokenAsync(idToken, tokenValidationParams);

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

    /// <inheritdoc />
    public async Task<Result<bool>> RedeemAuthCodeAndSaveRefreshTokenAsync(string authCode)
    {
        var tokenCallResult = await this.MSIdentityClient.CallTokenEndpointAsync(
            MSIdentityClient.GrantType.AuthorizationCode,
            authCode);

        if (tokenCallResult.IsError)
        {
            return Result<bool>.FromError(tokenCallResult.Error.Messsage, tokenCallResult.Error.Detail);
        }

        return Result<bool>.Success(true);
    }

    private static X509Certificate2 GetMSAppCert()
    {
        var certBase64 = Environment.GetEnvironmentVariable(Constants.SecretEnvVarNames.AadAuthorizationAppCert);
        return new X509Certificate2(Convert.FromBase64String(certBase64));
    }
}
