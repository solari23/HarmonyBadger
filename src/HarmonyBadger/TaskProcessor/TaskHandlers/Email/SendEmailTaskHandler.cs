using HarmonyBadger.ConfigModels.Tasks;
using Microsoft.Extensions.Configuration;
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
    public SendEmailTaskHandler(IEmailClient mailClient, IConfiguration appSettings)
    {
        this.MailClient = mailClient;
        this.AppSettings = appSettings;
    }

    private IEmailClient MailClient { get; }

    private IConfiguration AppSettings { get; }

    /// <inheritdoc />
    protected override async Task HandleAsync(SendEmailTask task, ILogger log)
    {
        var message = new EmailMessage
        {
            Subject = task.Subject,
            Body = task.Message,
            ToRecipients = task.ToRecipients,
            CCRecipients = task.CCRecipients,
            BccRecipients = task.BccRecipients,
            IsHighImportance = task.HighImportance,
            IsHtml = true,
        };

        var sender = string.IsNullOrWhiteSpace(task.Sender)
            ? this.AppSettings.DefaultEmailSenderAccount()
            : task.Sender;

        await this.MailClient.SendMailAsync(sender, message);
    }
}
