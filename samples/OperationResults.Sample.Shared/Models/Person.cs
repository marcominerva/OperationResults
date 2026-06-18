using System.ComponentModel.DataAnnotations;

namespace OperationResults.Sample.Shared.Models;

public class Person
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(50)]
    public string Email { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;
}
