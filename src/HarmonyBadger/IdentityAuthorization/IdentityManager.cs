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
    /// <param name="userEmail">The email of the user that is authorizing.</param>
    /// <param name="authCode">The authorization code to redeem.</param>
    /// <returns>A <see cref="Result"/> which indicates the outcome of the operation.</returns>
    Task<Result> RedeemAuthCodeAndSaveRefreshTokenAsync(string userEmail, string authCode);

    /// <summary>
    /// Uses a stored Refresh Token for the user to get an Access Token with the requested scopes.
    /// </summary>
    /// <param name="userEmail">The email of the user to get an Access Token for.</param>
    /// <returns>The Access Token, or an error.</returns>
    Task<Result<string>> GetAccessTokenForUserAsync(string userEmail);
}

public class IdentityManager : IIdentityManager
{
    private const string RefreshTokenType = "refresh_token";

    private static string HarmonyBadgerRedirectUri
        => $"https://{Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}/authorization";

    public IdentityManager(IMemoryCache memoryCache, IConfiguration appSettings, ITokenStorage tokenStorage)
    {
        this.MemoryCache = memoryCache;
        this.AppSettings = appSettings;
        this.TokenStorage = tokenStorage;

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

    private ITokenStorage TokenStorage { get; }

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

        return emailClaim;
    }

    /// <inheritdoc />
    public async Task<Result> RedeemAuthCodeAndSaveRefreshTokenAsync(string userEmail, string authCode)
    {
        var tokenCallResult = await this.MSIdentityClient.CallTokenEndpointAsync(
            MSIdentityClient.GrantType.AuthorizationCode,
            authCode);

        if (tokenCallResult.IsError)
        {
            return tokenCallResult.Error;
        }

        await this.TokenStorage.SaveTokenAsync(
            RefreshTokenType,
            userEmail,
            tokenCallResult.Value.Scope.Split(),
            tokenCallResult.Value.RefreshToken);

        return Result.SuccessResult;
    }

    /// <inheritdoc />
    public async Task<Result<string>> GetAccessTokenForUserAsync(string userEmail)
    {
        var getRefreshTokenResult = await this.TokenStorage.GetTokenAsync(RefreshTokenType, userEmail);
        if (getRefreshTokenResult.IsError)
        {
            return getRefreshTokenResult.Error;
        }

        var tokenResponse = await this.MSIdentityClient.CallTokenEndpointAsync(
            MSIdentityClient.GrantType.RefreshToken,
            getRefreshTokenResult.Value.Token);

        if (tokenResponse.IsError)
        {
            return tokenResponse.Error;
        }

        // Save the new RefreshToken returned by the IdP.
        await this.TokenStorage.SaveTokenAsync(
            RefreshTokenType,
            userEmail,
            tokenResponse.Value.Scope.Split(),
            tokenResponse.Value.RefreshToken);

        return tokenResponse.Value.AccessToken;
    }

    private static X509Certificate2 GetMSAppCert()
    {
        var certBase64 = Environment.GetEnvironmentVariable(Constants.SecretEnvVarNames.AadAuthorizationAppCert);
        return new X509Certificate2(Convert.FromBase64String(certBase64));
    }
}
