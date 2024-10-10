using System.ComponentModel.DataAnnotations;

namespace OperationResults.Sample.Shared.Models;

public class Person
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(30)]
    public string LastName { get; set; }

    [EmailAddress]
    [MaxLength(50)]
    public string Email { get; set; }

    public string City { get; set; }
}
