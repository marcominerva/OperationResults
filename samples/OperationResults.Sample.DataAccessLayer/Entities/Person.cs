namespace OperationResults.Sample.DataAccessLayer.Entities;

public class Person
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public DateTime CreateDate { get; set; }
}
