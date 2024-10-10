using OperationResults.Example.Shared.Models;

namespace OperationResults.Example.BusinessLayer.Services.Interfaces;

public interface IPeopleService
{
    Task<Result<IEnumerable<Person>>> GetAsync();

    Task<Result<Person>> GetAsync(Guid id);

    Task<Result<PersonWithImage>> GetWithImageAsync(Guid id);

    Task<Result<Person>> SaveAsync(Person person);

    Task<Result> DeleteAsync(Guid id);
}
