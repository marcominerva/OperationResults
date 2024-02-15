namespace OperationResults;

public class Result<T> : IGenericResult<T>
{
    public bool Success { get; }

    public T? Content { get; }

    public int FailureReason { get; }

    public Exception? Error { get; }

    private readonly string? errorMessage;
    public string? ErrorMessage => errorMessage ?? Error?.Message;

    private readonly string? errorDetail;
    public string? ErrorDetail => errorDetail ?? Error?.InnerException?.Message;

    public IEnumerable<ValidationError>? ValidationErrors { get; }

    internal Result(bool success = true, T? content = default, int failureReason = FailureReasons.None, string? message = null, string? detail = null, Exception? error = null, IEnumerable<ValidationError>? validationErrors = null)
    {
        Success = success;
        Content = content;
        FailureReason = failureReason;
        errorMessage = message;
        errorDetail = detail;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public bool TryGet(out T? value)
    {
        value = Content;
        return Success;
    }

    public static Result<T> Ok(T? content = default)
        => new(success: true, content);

    public static Result<T> Fail(int failureReason, ValidationError validationError)
        => new(false, failureReason: failureReason, validationErrors: [validationError]);

    public static Result<T> Fail(int failureReason, string message, ValidationError validationError)
        => new(false, failureReason: failureReason, message: message, validationErrors: [validationError]);

    public static Result<T> Fail(int failureReason, string message, string detail, ValidationError validationError)
        => new(false, failureReason: failureReason, message: message, detail: detail, validationErrors: [validationError]);

    public static Result<T> Fail(int failureReason, T content, ValidationError validationError)
        => new(false, failureReason: failureReason, content: content, validationErrors: [validationError]);

    public static Result<T> Fail(int failureReason, Exception? error, ValidationError validationError)
        => new(false, failureReason: failureReason, error: error, validationErrors: [validationError]);

    public static Result<T> Fail(int failureReason, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, validationErrors: validationErrors);

    public static Result<T> Fail(int failureReason, string message, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, message: message, validationErrors: validationErrors);

    public static Result<T> Fail(int failureReason, string message, string detail, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, message: message, detail: detail, validationErrors: validationErrors);

    public static Result<T> Fail(int failureReason, T content, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, content: content, validationErrors: validationErrors);

    public static Result<T> Fail(int failureReason, Exception? error, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, error: error, validationErrors: validationErrors);

    public static implicit operator Result<T>(T value)
        => Ok(value);

    public static implicit operator Result<T>(Result result)
        => new(result.Success, default, result.FailureReason, result.ErrorMessage, result.ErrorDetail, result.Error, result.ValidationErrors);

    public static bool operator true(Result<T> result)
        => result.Success;

    public static bool operator false(Result<T> result)
        => !result.Success;

    public static implicit operator bool(Result<T> result)
        => result.Success;
}
