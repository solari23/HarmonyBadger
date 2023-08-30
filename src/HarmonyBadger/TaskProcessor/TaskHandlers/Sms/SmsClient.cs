using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

public interface ISmsClient
{
    /// <summary>
    /// Sends an SMS message.
    /// </summary>
    /// <param name="phoneNumber">The phone number to send the message to.</param>
    /// <param name="message">The message to send.</param>
    Task SendMessageAsync(string phoneNumber, string message);
}

/// <summary>
/// Implementation of <see cref="ISmsClient"/> that sends email via Telesign's SMS API.
/// See Telesign Docs:
/// https://developer.telesign.com/enterprise/reference/sendsms
/// </summary>
public class TelesignSmsClient : ISmsClient, IDisposable
{
    private const string TelesignMessageApiEndpoint = "https://rest-ww.telesign.com/v1/messaging";

    public TelesignSmsClient()
    {
        this.HttpClient = new HttpClient();
    }

    private HttpClient HttpClient { get; }

    /// <inheritdoc />
    public async Task SendMessageAsync(string phoneNumber, string message)
    {
        var telesignCustomerId = Environment.GetEnvironmentVariable(Constants.SecretEnvVarNames.TelesignCustomerId);
        var telesignApiKey = Environment.GetEnvironmentVariable(Constants.SecretEnvVarNames.TelesignApiKey);

        var credential = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{telesignCustomerId}:{telesignApiKey}"));
        this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credential);

        var request = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "phone_number", phoneNumber },
            { "message", message },
            { "message_type", "ARN" },  // ARN = "Alerts, Reminders, and Notifications"
        });

        var response = await this.HttpClient.PostAsync(
            TelesignMessageApiEndpoint,
            request);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            throw new OperationFailedException(
                $"Calling Telesign SendSms API Failed\nHttpStatus: {response.StatusCode} ({(int)response.StatusCode})\nResponse body: {responseContent}");
        }
    }

    #region IDisposable Implementation

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                this.HttpClient?.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
