using HarmonyBadger.ConfigModels.Tasks;
using HarmonyBadger.IdentityAuthorization;
using Microsoft.Extensions.Logging;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// Handler for <see cref="TaskKind.ForceRefreshToken"/> tasks.
/// </summary>
public class ForceRefreshTokenTaskHandler : TaskHandlerBase<ForceRefreshTokenTask>
{
    /// <summary>
    /// Creates a new instance of the <see cref="ForceRefreshTokenTaskHandler"/> class.
    /// </summary>
    public ForceRefreshTokenTaskHandler(IConfigProvider configProvider, IIdentityManager identityManager)
        : base(configProvider)
    {
        this.IdentityManager = identityManager;
    }

    private IIdentityManager IdentityManager { get; }

    /// <inheritdoc />
    protected override async Task HandleAsync(ForceRefreshTokenTask task, ILogger log)
    {
        var errors = new List<string>();

        foreach (var token in task.TokensToRefresh)
        {
            var refreshResult = await this.IdentityManager.GetAccessTokenForUserAsync(token.UserEmail);

            if (refreshResult.IsError)
            {
                errors.Add($"Failed to refresh token for '{token.UserEmail}' due to error: {refreshResult.Error.Messsage}");
            }
            else
            {
                log.LogInformation($"Refreshed token for '{token.UserEmail}'");
            }
        }

        if (errors.Count > 0)
        {
            var error = $"Some token refreshes failed:\n" + string.Join('\n', errors);
            throw new Exception(error);
        }
    }
}
