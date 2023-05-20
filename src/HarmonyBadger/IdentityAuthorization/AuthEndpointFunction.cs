using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HarmonyBadger.IdentityAuthorization;

public static class AuthEndpointFunction
{
    [FunctionName("HarmonyBadger_AuthEndpoint_Get")]
    public static async Task<IActionResult> RunGetAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "authorization")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Executed GET");
    }

    [FunctionName("HarmonyBadger_AuthEndpoint_Post")]
    public static async Task<IActionResult> RunPostAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "authorization")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Executed POST");
    }
}
