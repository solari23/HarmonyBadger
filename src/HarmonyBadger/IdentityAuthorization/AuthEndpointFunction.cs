using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HarmonyBadger.IdentityAuthorization;

public class AuthEndpointFunction
{
    [FunctionName("HarmonyBadger_AuthEndpoint_Get")]
    public IActionResult RunGet(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "authorization")] HttpRequest req,
        ILogger log)
    {
        var authorizeQueryBuilder = new QueryStringBuilder();
        authorizeQueryBuilder.AddParameter("client_id", "49ad3364-fa8b-4b1f-9761-5a6bc32cacf2");
        authorizeQueryBuilder.AddParameter("response_type", "code+id_token", urlEncode: false);
        authorizeQueryBuilder.AddParameter("redirect_uri", $"https://{req.Host}/authorization");
        authorizeQueryBuilder.AddParameter("scope", "openid+email+offline_access+mail.send", urlEncode: false);
        authorizeQueryBuilder.AddParameter("response_mode", "form_post");
        authorizeQueryBuilder.AddParameter("prompt", "login");
        authorizeQueryBuilder.AddParameter("nonce", Guid.NewGuid().ToString());

        var authorizeUriBuilder = new UriBuilder("https://login.microsoftonline.com/common/oauth2/v2.0/authorize");
        authorizeUriBuilder.Query = authorizeQueryBuilder.Build();

        return new RedirectResult(authorizeUriBuilder.Uri.ToString());
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

        return new OkObjectResult("Refresh Token Saved.");
    }
}
