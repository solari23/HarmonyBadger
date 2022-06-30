using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace HarmonyBadger.ConfigModels;

/// <summary>
/// Extensions to simplify dealing with <see cref="IValidatableObject"/>.
/// </summary>
public static class IValidatableObjectExtensions
{
    /// <summary>
    /// Runs validation on the given <see cref="IValidatableObject"/> and throws an exception if it is invalid.
    /// </summary>
    /// <param name="obj">The object to validate.</param>
    /// <param name="argName">The name of the object argument (usually obtained via nameof operator).</param>
    /// <exception cref="ArgumentNullException">The object is null.</exception>
    /// <exception cref="ArgumentException">The object validation returned errors.</exception>
    public static void ThrowIfNotValid(
        this IValidatableObject obj,
        [CallerArgumentExpression("obj")] string argName = null)
    {
        ArgumentNullException.ThrowIfNull(obj, argName);

        var validationFailures = obj.Validate(new ValidationContext(obj)).ToList();

        if (validationFailures.Any())
        {
            var failures = string.Join(Environment.NewLine, validationFailures);
            throw new ArgumentException(
                $"Validation of '{obj.GetType().Name}' failed:{Environment.NewLine}{failures}");
        }
    }
}
