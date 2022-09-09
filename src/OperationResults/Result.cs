namespace OperationResults;

public class Result : IGenericResult
{
    public bool Success { get; }

    public FailureReason FailureReason { get; }

    public Exception? Error { get; }

    private readonly string? errorMessage;
    public string? ErrorMessage => errorMessage ?? Error?.Message;

    private readonly string? errorDetail;
    public string? ErrorDetail => errorDetail ?? Error?.InnerException?.Message;

    public IEnumerable<ValidationError>? ValidationErrors { get; }

    internal Result(bool success = true, FailureReason failureReason = FailureReason.None, string? message = null, string? detail = null, Exception? error = null, IEnumerable<ValidationError>? validationErrors = null)
    {
        Success = success;
        FailureReason = failureReason;
        errorMessage = message;
        errorDetail = detail;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public static Result Ok()
        => new(success: true);

    public static Result Fail(FailureReason failureReason, ValidationError validationError)
        => new(false, failureReason: failureReason, validationErrors: new ValidationError[] { validationError });

    public static Result Fail(FailureReason failureReason, string message, ValidationError validationError)
        => new(false, failureReason: failureReason, message: message, validationErrors: new ValidationError[] { validationError });

    public static Result Fail(FailureReason failureReason, string message, string detail, ValidationError validationError)
        => new(false, failureReason: failureReason, message: message, detail: detail, validationErrors: new ValidationError[] { validationError });

    public static Result Fail(FailureReason failureReason, Exception error, ValidationError validationError)
        => new(false, failureReason: failureReason, error: error, validationErrors: new ValidationError[] { validationError });

    public static Result Fail(FailureReason failureReason, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, validationErrors: validationErrors);

    public static Result Fail(FailureReason failureReason, string message, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, message: message, validationErrors: validationErrors);

    public static Result Fail(FailureReason failureReason, string message, string detail, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, message: message, detail: detail, validationErrors: validationErrors);

    public static Result Fail(FailureReason failureReason, Exception error, IEnumerable<ValidationError>? validationErrors = null)
        => new(false, failureReason: failureReason, error: error, validationErrors: validationErrors);
}
