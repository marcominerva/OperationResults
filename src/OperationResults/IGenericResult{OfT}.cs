namespace OperationResults;

public interface IGenericResult<T> : IGenericResult
{
    public T? Content { get; }

    public bool TryGet(out T? value);
}
