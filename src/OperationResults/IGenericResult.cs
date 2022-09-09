namespace OperationResults;

public interface IGenericResult
{
    bool Success { get; }

    int FailureReason { get; }

    Exception? Error { get; }

    string? ErrorDetail { get; }

    string? ErrorMessage { get; }

    IEnumerable<ValidationError>? ValidationErrors { get; }
}
