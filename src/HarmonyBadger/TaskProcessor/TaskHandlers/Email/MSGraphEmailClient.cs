using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using HarmonyBadger.IdentityAuthorization;

namespace HarmonyBadger.TaskProcessor.TaskHandlers;

/// <summary>
/// Encapsualtes the details of an email message to be send.
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// The subject of the email.
    /// </summary>
    public string Subject { get; init; }

    /// <summary>
    /// The body of the email.
    /// </summary>
    public string Body { get; init; }

    /// <summary>
    /// Whether or not the body of the email is HTML.
    /// </summary>
    public bool IsHtml { get; init; }

    /// <summary>
    /// The list of To-line recipients of the email.
    /// </summary>
    public IReadOnlyList<string> ToRecipients { get; init; }

    /// <summary>
    /// The list of CC recipients of the email.
    /// </summary>
    public IReadOnlyList<string> CCRecipients { get; init; }

    /// <summary>
    /// The list of BCC recipients of the email.
    /// </summary>
    public IReadOnlyList<string> BccRecipients { get; init; }

    /// <summary>
    /// Whether or not the email should be marked as high importance.
    /// </summary>
    public bool IsHighImportance { get; init; }
}

public interface IEmailClient
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="sender">The sender's email address to use.</param>
    /// <param name="message">The <see cref="EmailMessage"/> to send.</param>
    /// <remarks>
    /// In order to send email, the <paramref name="sender"/> must have granted
    /// OAuth authorization to Harmony Badger via the authorization endpoint.
    /// </remarks>
    Task SendMailAsync(string sender, EmailMessage message);
}

/// <summary>
/// Implementation of <see cref="IEmailClient"/> that sends email via MSGraph SendMail API.
/// MSGraph documentation for more information:
/// https://learn.microsoft.com/en-us/graph/api/user-sendmail
/// </summary>
public class MSGraphEmailClient : IEmailClient, IDisposable
{
    private const string MSGraphSendMailApiEndpoint = "https://graph.microsoft.com/v1.0/me/sendMail";

    /// <summary>
    /// Creates a new instance of the <see cref="MSGraphEmailClient"/> class.
    /// </summary>
    public MSGraphEmailClient(IIdentityManager identityManager)
    {
        this.IdentityManager = identityManager;
        this.HttpClient = new HttpClient();
    }

    private IIdentityManager IdentityManager { get; }

    private HttpClient HttpClient { get; }

    /// <inheritdoc />
    public async Task SendMailAsync(string sender, EmailMessage message)
    {
        var getAccessTokenResult = await this.IdentityManager.GetAccessTokenForUserAsync(sender);
        if (getAccessTokenResult.IsError)
        {
            getAccessTokenResult.Error.Throw("Acquring access token to send mail failed.");
        }

        this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", getAccessTokenResult.Value);

        var request = new SendMailRequest
        {
            Message = new Message
            {
                Subject = message.Subject,
                Body = new Body
                {
                    ContentType = message.IsHtml ? ContentType.Html : ContentType.Text,
                    Content = message.Body,
                },
                ToRecipients = ToRequestRecipientFormat(message.ToRecipients),
                CCRecipients = ToRequestRecipientFormat(message.CCRecipients),
                BccRecipients = ToRequestRecipientFormat(message.BccRecipients),
                Importance = message.IsHighImportance ? Importance.High : Importance.Normal,
            },
        };
        var requestJson = JsonSerializer.Serialize(request);

        var response = await this.HttpClient.PostAsync(
            MSGraphSendMailApiEndpoint,
            new StringContent(requestJson, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
    }

    private static Recipient[] ToRequestRecipientFormat(IEnumerable<string> recipients)
        => recipients is null || !recipients.Any()
            ? null
            : recipients.Select(r => new Recipient { EmailAddress = new EmailAddress { Address = r } }).ToArray();

    #region Request Classes

    private class SendMailRequest
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    private class Message
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("body")]
        public Body Body { get; set; }

        [JsonPropertyName("toRecipients")]
        public Recipient[] ToRecipients { get; set; }

        [JsonPropertyName("ccRecipients")]
        public Recipient[] CCRecipients { get; set; }

        [JsonPropertyName("bccRecipients")]
        public Recipient[] BccRecipients { get; set; }

        [JsonPropertyName("importance")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Importance Importance { get; set; } = Importance.Normal;
    }

    private class Body
    {
        [JsonPropertyName("contentType")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ContentType ContentType { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    private class Recipient
    {
        [JsonPropertyName("emailAddress")]
        public EmailAddress EmailAddress { get; set; }
    }

    private class EmailAddress
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }
    }

    private enum ContentType
    {
        Text,
        Html,
    }

    private enum Importance
    {
        Low,
        Normal,
        High,
    }

    #endregion

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
