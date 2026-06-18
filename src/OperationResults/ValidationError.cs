namespace OperationResults;

/// <summary>
/// Describes a validation problem associated with a field, parameter, or business rule rejected by an operation.
/// </summary>
/// <param name="Name">The field, parameter, or rule name that identifies where the validation problem occurred.</param>
/// <param name="Message">The validation message that explains why the value or rule was rejected.</param>
public record class ValidationError(string Name, string Message);