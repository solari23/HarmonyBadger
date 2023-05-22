using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HarmonyBadger.ConfigModels.Tasks;

/// <summary>
/// An internal utility task that forces HarmonyBadger to refresh tokens in storage so that they do not expire.
/// </summary>
public class ForceRefreshTokenTask : ITask, IValidatableObject, IJsonOnDeserialized
{
    /// <inheritdoc />
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaskKind TaskKind => TaskKind.ForceRefreshToken;

    /// <summary>
    /// The list of tokens to refresh.
    /// </summary>
    public List<TokenDetails> TokensToRefresh { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (this.TokensToRefresh is null || this.TokensToRefresh.Count == 0)
        {
            yield return new ValidationResult(
                $"{nameof(ForceRefreshTokenTask)} must specify non-empty list property '{nameof(TokensToRefresh)}'");
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}

/// <summary>
/// Information about a token to force refresh.
/// </summary>
public class TokenDetails : IValidatableObject, IJsonOnDeserialized
{
    /// <summary>
    /// The email of the user that this token is for.
    /// </summary>
    public string UserEmail { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (string.IsNullOrEmpty(this.UserEmail))
        {
            yield return new ValidationResult(
                $"{nameof(TokenDetails)} in ForceRefreshToken task is missing field '{nameof(this.UserEmail)}'");
        }
    }

    /// <inheritdoc />
    public void OnDeserialized() => this.ThrowIfNotValid();
}
