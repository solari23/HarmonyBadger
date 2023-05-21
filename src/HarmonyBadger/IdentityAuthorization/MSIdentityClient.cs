using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace HarmonyBadger.IdentityAuthorization;

/// <summary>
/// Integrates with the MS Identity Platform.
///
/// This is used in place of MSAL to cover for shortcomings in MSAL's API that make it
/// not usable for HarmonyBadger's scenario. MSAL expects to be able to retrieve and
/// cache refresh tokens without exposing them to the developer. This doesn't work well
/// to handle the offline_access scenario where the app is behaving as a daemon running
/// on user data in the background.
/// </summary>
public class MSIdentityClient
{
    public enum GrantType
    {
        AuthorizationCode,
        RefreshToken,
    }

    public const string Authority = "https://login.microsoftonline.com/common/";

    public MSIdentityClient(string appId, string redirectUri, X509Certificate2 appCertificate)
    {
        this.AppId = appId;
        this.RedirectUri = redirectUri;
        this.AppCertificate = appCertificate;

        this.HttpClient = new HttpClient();
    }

    private string AppId { get; }

    private string RedirectUri { get; }

    private X509Certificate2 AppCertificate { get; }

    private HttpClient HttpClient { get; }

    public async Task<OpenIdConnectConfiguration> GetOidcConfigAsync()
    {
        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());
        var config = await configManager.GetConfigurationAsync();
        return config;
    }

    public Uri GetAuthorizationRequestUri(IEnumerable<string> scopes)
    {
        var scopeString = string.Join(' ', scopes);

        var authorizeQueryBuilder = new QueryStringBuilder();
        authorizeQueryBuilder.AddParameter("client_id", this.AppId);
        authorizeQueryBuilder.AddParameter("response_type", "code id_token");
        authorizeQueryBuilder.AddParameter("redirect_uri", this.RedirectUri);
        authorizeQueryBuilder.AddParameter("scope", scopeString);
        authorizeQueryBuilder.AddParameter("response_mode", "form_post");
        authorizeQueryBuilder.AddParameter("prompt", "consent");
        authorizeQueryBuilder.AddParameter("nonce", Guid.NewGuid().ToString());

        var authorizeUriBuilder = new UriBuilder("https://login.microsoftonline.com/common/oauth2/v2.0/authorize");
        authorizeUriBuilder.Query = authorizeQueryBuilder.Build();

        return authorizeUriBuilder.Uri;
    }

    public async Task<Result<TokenResponse>> CallTokenEndpointAsync(GrantType grantType, string grant)
    {
        var requestParams = new Dictionary<string, string>
        {
            { "client_id" , this.AppId },
            { "redirect_uri", this.RedirectUri },
            { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
            { "client_assertion", this.GetClientAssertion() },
        };

        switch (grantType)
        {
            case GrantType.RefreshToken:
                requestParams.Add("grant_type", "refresh_token");
                requestParams.Add("refresh_token", grant);
                break;

            case GrantType.AuthorizationCode:
                requestParams.Add("grant_type", "authorization_code");
                requestParams.Add("code", grant);
                break;

            default:
                throw new InvalidOperationException($"Unexpected grant type '{grantType}'");
        }

        var request = new FormUrlEncodedContent(requestParams);
        var response = await this.HttpClient.PostAsync(
            "https://login.microsoftonline.com/common/oauth2/v2.0/token",
            request);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var parsedResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);

            return Result<TokenResponse>.Success(parsedResponse);
        }
        else
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var parsedResponse = JsonSerializer.Deserialize<ErrorResponse>(responseBody);

            return Result<TokenResponse>.FromError(
                $"Call to token endpoint failed with error '{parsedResponse.Error}'",
                parsedResponse.ErrorDescription);
        }
    }

    private string GetClientAssertion()
    {
        // No need to add exp, nbf as JsonWebTokenHandler will add them by default.
        var claims = new Dictionary<string, object>
        {
            { "aud", "https://login.microsoftonline.com/common/v2.0" },
            { "iss", this.AppId },
            { "sub", this.AppId },
            { "jti", Guid.NewGuid().ToString() },
        };

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            SigningCredentials = new X509SigningCredentials(this.AppCertificate)
        };

        var handler = new JsonWebTokenHandler();
        var signedClientAssertion = handler.CreateToken(securityTokenDescriptor);
        return signedClientAssertion;
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }

    private class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("correlation_id")]
        public string CorrelationId { get; set; }
    }
}
