using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels.Tasks;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// Handler for <see cref="TaskKind.SendEmail"/> tasks.
/// </summary>
public class SendEmailTaskHandler : TaskHandlerBase<SendEmailTask>
{
    /// <summary>
    /// Creates a new instance of the <see cref="SendEmailTaskHandler"/> class.
    /// </summary>
    public SendEmailTaskHandler(IEmailClient mailClient, IConfiguration appSettings, ITemplateEngine templateEngine)
    {
        this.MailClient = mailClient;
        this.AppSettings = appSettings;
        this.TemplateEngine = templateEngine;
    }

    private IEmailClient MailClient { get; }

    private IConfiguration AppSettings { get; }

    private ITemplateEngine TemplateEngine { get; }

    /// <inheritdoc />
    protected override async Task HandleAsync(SendEmailTask task, ILogger log)
    {
        var renderedMessageBody = await this.TemplateEngine.RenderTemplatedMessageAsync(task);
        var message = new EmailMessage
        {
            Subject = task.Subject,
            Body = renderedMessageBody,
            ToRecipients = task.ToRecipients,
            CCRecipients = task.CCRecipients,
            BccRecipients = task.BccRecipients,
            IsHighImportance = task.HighImportance,
            IsHtml = task.IsHtml
                || (task.TemplateFilePath is not null && task.TemplateFilePath.EndsWith(".html", StringComparison.OrdinalIgnoreCase)),
        };

        var sender = string.IsNullOrWhiteSpace(task.Sender)
            ? this.AppSettings.DefaultEmailSenderAccount()
            : task.Sender;

        await this.MailClient.SendMailAsync(sender, message);
    }
}
