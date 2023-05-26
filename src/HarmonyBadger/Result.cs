using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HarmonyBadger;

/// <summary>
/// Encapsulates the outcome of an operation.
/// </summary>
public class Result
{
    /// <summary>
    /// The default <see cref="Result"/> instance indicating a successful outcome.
    /// </summary>
    public static readonly Result SuccessResult = new (null);

    /// <summary>
    /// Creates a <see cref="Result"/> for an operation that successfully completed.
    /// </summary>
    /// <returns>The <see cref="Result"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Success() => SuccessResult;

    /// <summary>
    /// Create a <see cref="Result"/> representing an operation that failed.
    /// </summary>
    /// <param name="message">A summary message that describes the error.</param>
    /// <returns>The <see cref="Result"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result FromError(string message)
        => FromError(message, null, null);

    /// <summary>
    /// Create a <see cref="Result"/> representing an operation that failed.
    /// </summary>
    /// <param name="message">A summary message that describes the error.</param>
    /// <param name="detail">Detailed information about the error.</param>
    /// <returns>The <see cref="Result"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result FromError(string message, string detail)
        => FromError(message, detail, null);

    /// <summary>
    /// Create a <see cref="Result"/> representing an operation that failed.
    /// </summary>
    /// <param name="message">A summary message that describes the error.</param>
    /// <param name="exception">The exception that is the source of the error (if applicable).</param>
    /// <returns>The <see cref="Result"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result FromError(string message, Exception exception)
        => FromError(message, null, exception);

    /// <summary>
    /// Create a <see cref="Result"/> representing an operation that failed.
    /// </summary>
    /// <param name="message">A summary message that describes the error.</param>
    /// <param name="detail">Detailed information about the error.</param>
    /// <param name="exception">The exception that is the source of the error (if applicable).</param>
    /// <returns>The <see cref="Result"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result FromError(string message, string detail, Exception exception)
        => FromError(errorInfoFactory(message, detail, exception));

    /// <summary>
    /// Create a <see cref="Result{TValue}"/> representing an operation that failed.
    /// </summary>
    /// <param name="error">Diagnostic infromation about the error.</param>
    /// <returns>The <see cref="Result"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result FromError(ErrorInfo error)
        => new (error);

    /// <summary>
    /// Casts the error to an instance of <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error to cast.</param>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(ErrorInfo error) => FromError(error);

    protected static Func<string, string, Exception, ErrorInfo> errorInfoFactory;

    static Result()
    {
        // Ensure the nested ErrorInfo type's static CTOR has run and populared errorInfoFactory.
        RuntimeHelpers.RunClassConstructor(typeof(ErrorInfo).TypeHandle);
    }

    protected Result(ErrorInfo error)
    {
        this.Error = error;
    }

    /// <summary>
    /// Diagnostic information in the case where this <see cref="Result"/>
    /// represents an error, otherwise null.
    /// </summary>
    public ErrorInfo Error { get; }

    /// <summary>
    /// True if this <see cref="Result"/> represents a successful outcome, false otherwise.
    /// </summary>
    public bool IsSuccess => this.Error is null;

    /// <summary>
    /// True if this <see cref="Result"/> represents an error, false otherwise.
    /// </summary>
    public bool IsError => this.Error is not null;

    /// <summary>
    /// Encapsulates diagnostic information about an error result.
    /// </summary>
    public class ErrorInfo
    {
        static ErrorInfo()
        {
            // This pattern ensures that only the parent Result type can construct instances of this type.
            errorInfoFactory = (m, d, e) => new ErrorInfo(m, d, e);
        }

        /// <summary>
        /// Hidden CTOR. Exposed only to the containing class via <see cref="errorInfoFactory"/>.
        /// </summary>
        private ErrorInfo(string messsage, string detail, Exception exception)
        {
            this.Message = messsage;
            this.Detail = detail;
            this.Exception = exception;
        }

        /// <summary>
        /// A summary message that describes the error.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Detailed information about the error.
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// The exception that is the source of the error (if applicable).
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Causes an exception of type <see cref="OperationFailedException"/> to be thrown.
        /// The exception will contain diagnostic information from this <see cref="ErrorInfo"/> object.
        /// </summary>
        /// <param name="contextMessage">An optional message to add to as a prefix to the exception message.</param>
        public void Throw(string contextMessage = null)
        {
            contextMessage ??= string.Empty;
            throw new OperationFailedException($"{contextMessage}\nError: {this.Message}\nDetails: {this.Detail}", this.Exception);
        }
    }
}

/// <summary>
/// Encapsulates the result of an operation, which may be either some result of
/// type <typeparamref name="TValue"/>, or some error.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class Result<TValue> : Result
{
    /// <summary>
    /// Creates a <see cref="Result{TValue}"/> for an operation that successfully
    /// completed and resulted in a value of type <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="value">The successful result value.</param>
    /// <returns>The <see cref="Result{TValue}"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TValue> Success(TValue value)
        => new (value, null);

    /// <summary>
    /// Create a <see cref="Result{TValue}"/> representing an operation that failed.
    /// </summary>
    /// <param name="message">A summary message that describes the error.</param>
    /// <returns>The <see cref="Result{TValue}"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static new Result<TValue> FromError(string message)
        => FromError(message, null, null);

    /// <summary>
    /// Create a <see cref="Result{TValue}"/> representing an operation that failed.
    /// </summary>
    /// <param name="message">A summary message that describes the error.</param>
    /// <param name="detail">Detailed information about the error.</param>
    /// <returns>The <see cref="Result{TValue}"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static new Result<TValue> FromError(string message, string detail)
        => FromError(message, detail, null);

    /// <summary>
    /// Create a <see cref="Result{TValue}"/> representing an operation that failed.
    /// </summary>
    /// <param name="message">A summary message that describes the error.</param>
    /// <param name="exception">The exception that is the source of the error (if applicable).</param>
    /// <returns>The <see cref="Result{TValue}"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static new Result<TValue> FromError(string message, Exception exception)
        => FromError(message, null, exception);

    /// <summary>
    /// Create a <see cref="Result{TValue}"/> representing an operation that failed.
    /// </summary>
    /// <param name="message">A summary message that describes the error.</param>
    /// <param name="detail">Detailed information about the error.</param>
    /// <param name="exception">The exception that is the source of the error (if applicable).</param>
    /// <returns>The <see cref="Result{TValue}"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static new Result<TValue> FromError(string message, string detail, Exception exception)
        => FromError(errorInfoFactory(message, detail, exception));

    /// <summary>
    /// Create a <see cref="Result{TValue}"/> representing an operation that failed.
    /// </summary>
    /// <param name="error">Diagnostic infromation about the error.</param>
    /// <returns>The <see cref="Result{TValue}"/>.</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static new Result<TValue> FromError(ErrorInfo error)
        => new (default, error);

    /// <summary>
    /// Casts the <see cref="Result{TValue}"/> to the underlying <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="result">The <see cref="Result{TValue}"/> to cast.</param>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="Result{TValue}"/> represents an error and does not contain a value.
    /// </exception>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator TValue(Result<TValue> result) => result.Value;

    /// <summary>
    /// Casts the given <typeparamref name="TValue"/> value to a <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="result">The value to cast.</param>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    /// Casts the error to an instance of <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="error">The error to cast.</param>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<TValue>(ErrorInfo error) => FromError(error);

    private Result(TValue value, ErrorInfo error) : base(error)
    {
        this.value = value;
    }

    private readonly TValue value;

    /// <summary>
    /// Returns the resulting value of the operation in the case of success.
    /// Otherwise, throws <see cref="InvalidOperationException"/> if the
    /// <see cref="Result{TValue}"/> represents an error.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="Result{TValue}"/> represents an error and does not contain a value.
    /// </exception>
    public TValue Value
    {
        get
        {
            if (this.IsError)
            {
                throw new InvalidOperationException(
                    "The result indicates an error; no value is accessible.");
            }

            return value;
        }
    }
}