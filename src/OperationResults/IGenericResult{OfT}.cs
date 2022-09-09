namespace OperationResults;

public interface IGenericResult<T> : IGenericResult
{
    public T? Content { get; }
}
