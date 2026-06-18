namespace OperationResults.Sample.Shared.Models;

public class PersonWithImage
{
    public Person Person { get; set; } = new();

    public byte[]? Image { get; set; }
}