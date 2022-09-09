using OperationResults.WebApi.Models;

namespace OperationResults.WebApi.Services;

public class PeopleService
{
    public async Task<Result<IEnumerable<Person>>> GetAsync(string queryText)
    {
        await Task.Delay(100);
        var people = new List<Person>();

        return people;
    }

    public async Task<Result<Person>> GetAsync(Guid id)
    {
        await Task.Delay(100);

        return Result.Fail(FailureReasons.ItemNotFound);
    }
}
