namespace OperationResults.AspNetCore;

/// <summary>
/// Defines how validation errors are serialized in ASP.NET Core problem responses created from operation results.
/// </summary>
/// <remarks>
/// The format lets applications choose between the default ASP.NET Core model-state shape and a flat list of <see cref="ValidationError"/> values while keeping business services independent from HTTP response contracts.
/// </remarks>
public enum ErrorResponseFormat
{
    /// <summary>
    /// Groups validation messages by field or rule name, matching the common ASP.NET Core validation problem details shape.
    /// </summary>
    Default,

    /// <summary>
    /// Emits validation errors as a sequence of <see cref="ValidationError"/> values for clients that prefer a uniform list payload.
    /// </summary>
    List
}
