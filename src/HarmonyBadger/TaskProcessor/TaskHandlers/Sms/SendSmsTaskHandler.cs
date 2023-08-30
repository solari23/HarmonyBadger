using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using HarmonyBadger.ConfigModels.Tasks;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// Handler for <see cref="TaskKind.SendSms"/> tasks.
/// </summary>
public class SendSmsTaskHandler : TaskHandlerBase<SendSmsTask>
{
    /// <summary>
    /// Creates a new instance of the <see cref="SendSmsTaskHandler"/> class.
    /// </summary>
    public SendSmsTaskHandler(ISmsClient smsClient)
    {
        this.SmsClient = smsClient;
    }

    private ISmsClient SmsClient { get; }

    /// <inheritdoc />
    protected override async Task HandleAsync(SendSmsTask task, ILogger log)
    {
        log.LogInformation($"[SendSmsTaskHandler] Sending message to '{task.PhoneNumber}'");
        await this.SmsClient.SendMessageAsync(task.PhoneNumber, task.Message);
    }
}
