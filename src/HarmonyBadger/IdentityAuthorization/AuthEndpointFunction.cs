using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HarmonyBadger.IdentityAuthorization;

public class AuthEndpointFunction
{
    public AuthEndpointFunction(IConfiguration appSettings, IIdentityManager identityManager)
    {
        this.AppSettings = appSettings;
        this.IdentityManager = identityManager;
    }

    private IConfiguration AppSettings { get; }

    private IIdentityManager IdentityManager { get; }

    [FunctionName("HarmonyBadger_AuthEndpoint_Get")]
    public IActionResult RunGet(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "authorization")] HttpRequest req)
    {
        var authorizeUri = this.IdentityManager.GetAuthorizationRequestUri(
            new[] { "openid", "email", "offline_access", "mail.send" });
        return new RedirectResult(authorizeUri.ToString());
    }

    [FunctionName("HarmonyBadger_AuthEndpoint_Post")]
    public async Task<IActionResult> RunPostAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "authorization")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        using var formReader = new FormReader(req.Body);
        var requestForm = await formReader.ReadFormAsync();

        if (requestForm.ContainsKey("error"))
        {
            var error = requestForm["error"].First();
            var errorDescription = requestForm.ContainsKey("error_description")
                ? requestForm["error_description"].First()
                : string.Empty;
            var errorString = $"IdP reported an error during authorization.\nError: {error}\nDescription: {errorDescription}";

            // Return the error as a 200 so it'll render in browser.
            // The HTTP status doesn't actually matter here.
            log.LogError(errorString);
            return new OkObjectResult(errorString);
        }

        var idToken = requestForm["id_token"].First();
        var authCode = requestForm["code"].First();

        var idTokenValidationResult = await this.IdentityManager.ValidateIsAuthorizedUserAsync(idToken);
        if (idTokenValidationResult.IsError)
        {
            log.LogError(
                idTokenValidationResult.Error.Exception,
                $"id_token validation failed with message: {idTokenValidationResult.Error.Messsage}");
            return new UnauthorizedResult();
        }

        var authorizedUserEmail = idTokenValidationResult.Value;

        var redeemResult = await this.IdentityManager.RedeemAuthCodeAndSaveRefreshTokenAsync(authCode);
        if (redeemResult.IsError)
        {
            log.LogError($"Redeeming auth code failed due to error: {redeemResult.Error.Messsage}\n Detail: {redeemResult.Error.Detail}");
        }

        return new OkObjectResult($"RefreshToken for {authorizedUserEmail} saved.");
    }

    [FunctionName("HarmonyBadger_AuthEndpoint_Test")]
    public IActionResult Test(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "test")] HttpRequest req)
    {
        // Temporary function to help validate appsettings setup.
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"TestSetting1 => {this.AppSettings.GetValue<string>("TestSetting1")}");
        sb.AppendLine($"TestSetting2 => {this.AppSettings.GetValue<string>("TestSetting2")}");
        sb.AppendLine($"TestSetting3 => {this.AppSettings.GetValue<string>("TestSetting3")}");
        return new OkObjectResult(sb.ToString());
    }
}
