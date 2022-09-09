namespace OperationResults;

public class ValidationError
{
    public string Name { get; }

    public string Message { get; }

    public ValidationError(string name, string message)
    {
        Name = name[(name.LastIndexOf('.') + 1)..];
        Message = message;
    }
}
