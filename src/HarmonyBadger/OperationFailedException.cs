namespace HarmonyBadger;

/// <summary>
/// An exception that indicates that an operation failed.
/// Typically, this is used to convert a <see cref="Result.ErrorInfo"/> into a throwable exception.
/// </summary>
public sealed class OperationFailedException : Exception
{
    /// <inheritdoc />
    public OperationFailedException(string message) : base(message)
    {
        // Empty.
    }

    /// <inheritdoc />
    public OperationFailedException(string message, Exception innerException) : base(message, innerException)
    {
        // Empty
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationFailedException"/> that populates message and inner exception
    /// infromation from the given <see cref="Result.ErrorInfo"/> object.
    /// </summary>
    /// <param name="errorInfo"></param>
    public OperationFailedException(Result.ErrorInfo errorInfo)
        : this ($"Error: {errorInfo.Message}\nDetail: {errorInfo.Detail}", errorInfo.Exception)
    {
        // Empty.
    }
}
