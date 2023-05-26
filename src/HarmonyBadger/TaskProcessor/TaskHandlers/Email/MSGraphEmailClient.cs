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
    /// <returns>A result indicating the status of the operation.</returns>
    /// <remarks>
    /// In order to send email, the <paramref name="sender"/> must have granted
    /// OAuth authorization to Harmony Badger via the authorization endpoint.
    /// </remarks>
    Task<Result> SendMailAsync(string sender, EmailMessage message);
}

/// <summary>
/// Implementation of <see cref="IEmailClient"/> that sends email via MSGraph SendMail API.
/// MSGraph documentation for more information:
/// https://learn.microsoft.com/en-us/graph/api/user-sendmail
/// </summary>
public class MSGraphEmailClient : IEmailClient, IDisposable
{
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
    public async Task<Result> SendMailAsync(string sender, EmailMessage message)
    {
        throw new NotImplementedException();
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
