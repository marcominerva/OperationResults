using System.Diagnostics.CodeAnalysis;

namespace OperationResults;

public interface IGenericResult<T> : IGenericResult
{
    public T? Content { get; }

    public bool TryGetContent([NotNullWhen(returnValue: true)] T? content);
}
