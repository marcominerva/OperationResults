using OperationResults.WebApi.Models;

namespace OperationResults.WebApi.Services;

public class PeopleService
{
    public async Task<Result<IEnumerable<Person>>> GetAsync(string queryText)
    {
        await Task.Delay(100);
        _ = new List<Person>();

        var errors = new List<ValidationError>
        {
            new(nameof(Person.FirstName), "Il nome è obbligatorio"),
            new(nameof(Person.LastName), "Il cognome è obbligatorio")
        };

        return Result.Fail(FailureReasons.ClientError, errors);
    }

    public async Task<Result<Person>> GetAsync(Guid id)
    {
        await Task.Delay(100);

        return Result.Fail(FailureReasons.ItemNotFound);
    }
}
