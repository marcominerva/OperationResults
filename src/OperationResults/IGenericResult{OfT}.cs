using System.Diagnostics.CodeAnalysis;

namespace OperationResults;

/// <summary>
/// Exposes the transport-neutral metadata and optional content shared by typed operation results.
/// </summary>
/// <typeparam name="T">The type of content produced when the operation succeeds.</typeparam>
public interface IGenericResult<T> : IGenericResult
{
    /// <summary>
    /// Gets the content produced by a successful operation.
    /// </summary>
    public T? Content { get; }

    /// <summary>
    /// Attempts to read the content while preserving nullable-flow information for callers.
    /// </summary>
    /// <param name="content">When this method returns <see langword="true"/>, contains the successful operation content.</param>
    /// <returns><see langword="true"/> when content is available; otherwise, <see langword="false"/>.</returns>
    public bool TryGetContent([NotNullWhen(returnValue: true)] out T? content);
}
