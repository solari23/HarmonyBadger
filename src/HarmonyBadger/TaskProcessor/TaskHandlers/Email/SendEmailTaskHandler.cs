using HarmonyBadger.ConfigModels.Tasks;
using Microsoft.Extensions.Logging;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// Handler for <see cref="TaskKind.SendEmail"/> tasks.
/// </summary>
public class SendEmailTaskHandler : TaskHandlerBase<SendEmailTask>
{
    /// <summary>
    /// Creates a new instance of the <see cref="SendEmailTaskHandler"/> class.
    /// </summary>
    public SendEmailTaskHandler(IEmailClient mailClient)
    {
        this.MailClient = mailClient;
    }

    private IEmailClient MailClient { get; }

    /// <inheritdoc />
    protected override async Task HandleAsync(SendEmailTask task, ILogger log)
    {
        log.LogInformation("<TODO> Sending email!");
    }
}
