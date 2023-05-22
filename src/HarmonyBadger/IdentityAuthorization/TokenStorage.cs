using System.Security.Cryptography;

using Azure;
using Azure.Data.Tables;

namespace HarmonyBadger.IdentityAuthorization;

public interface ITokenStorage
{
    /// <summary>
    /// Persists the given token to storage.
    /// </summary>
    /// <param name="tokenType">The type of token. This is used as a partition key.</param>
    /// <param name="userEmail">The email of the user that the token is associated with.</param>
    /// <param name="scopes">The OAuth scopes that the token has been authorized for.</param>
    /// <param name="token">The token to store.</param>
    Task SaveTokenAsync(string tokenType, string userEmail, IEnumerable<string> scopes, string token);

    /// <summary>
    /// Gets a token from storage.
    /// </summary>
    /// <param name="tokenType">The type of token. This is used as a partition key.</param>
    /// <param name="userEmail">The email of the user whose token to retrieve.</param>
    /// <returns>A result that either contains the <see cref="TokenInfo"/> or an error.</returns>
    Task<Result<TokenInfo>> GetTokenAsync(string tokenType, string userEmail);
}

public class TokenStorage : ITokenStorage
{
    private const string TableName = "HarmonyBadgerUserTokens";

    private readonly Lazy<TableClient> storageClient = new (() =>
    {
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        var client = new TableClient(connectionString, TableName);
        client.CreateIfNotExists();
        return client;
    });

    private static readonly Lazy<Aes> TokenEncryptionKey = new (() =>
    {
        var encodedKey = Environment.GetEnvironmentVariable(Constants.SecretEnvVarNames.TokenEncryptionKey);
        var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(encodedKey);
        return aes;
    });

    /// <inheritdoc />
    public async Task SaveTokenAsync(string tokenType, string userEmail, IEnumerable<string> scopes, string token)
    {
        var tokenInfoEntity = new TokenInfoEntity
        {
            PartitionKey = tokenType,
            RowKey = NormalizeEmail(userEmail),
            UserEmail = userEmail,
            Scopes = string.Join(' ', scopes),
            EncryptedToken = CryptoHelper.EncryptDataAes256(TokenEncryptionKey.Value, token),
        };

        var response = await this.storageClient.Value.UpsertEntityAsync(tokenInfoEntity);
        if (response.IsError)
        {
            throw new Exception($"Failed to store token for user '{userEmail}'\nHTTP Status: {response.Status}\nHTTP Reason: {response.ReasonPhrase}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<TokenInfo>> GetTokenAsync(string tokenType, string userEmail)
    {
        var normalizedEmail = NormalizeEmail(userEmail);
        var getResult = await this.storageClient.Value.GetEntityIfExistsAsync<TokenInfoEntity>(
            tokenType,
            normalizedEmail);

        if (!getResult.HasValue)
        {
            return Result<TokenInfo>.FromError($"No {tokenType} token is stored for user {userEmail}");
        }

        var tokenInfoEntity = getResult.Value;
        var tokenInfo = new TokenInfo
        {
            UserEmail = tokenInfoEntity.UserEmail,
            Scopes = tokenInfoEntity.Scopes.Split(),
            Token = CryptoHelper.DecryptDataAes256(TokenEncryptionKey.Value, tokenInfoEntity.EncryptedToken),
        };

        return tokenInfo;
    }

    private static string NormalizeEmail(string email) => email.ToLowerInvariant();

    private class TokenInfoEntity : ITableEntity
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public string UserEmail { get; set; }

        public string Scopes { get; set; }

        public string EncryptedToken { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}

public class TokenInfo
{
    public string UserEmail { get; init; }

    public IReadOnlyCollection<string> Scopes { get; init; }

    public string Token { get; init; }
}
