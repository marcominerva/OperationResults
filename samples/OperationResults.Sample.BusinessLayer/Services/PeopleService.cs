using Microsoft.EntityFrameworkCore;
using OperationResults.Sample.BusinessLayer.Services.Interfaces;
using OperationResults.Sample.DataAccessLayer;
using OperationResults.Sample.Shared.Models;
using Entities = OperationResults.Sample.DataAccessLayer.Entities;

namespace OperationResults.Sample.BusinessLayer.Services;

public class PeopleService(ApplicationDbContext dbContext, IImageService imageService) : IPeopleService
{
    public async Task<Result<IEnumerable<Person>>> GetAsync()
    {
        var people = await dbContext.People.AsNoTracking()
            .OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
            .Select(p => new Person
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.Email,
                City = p.City
            })
            .ToListAsync();

        return people;
    }

    public async Task<Result<Person>> GetAsync(Guid id)
    {
        var dbPerson = await dbContext.People.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (dbPerson is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        var person = new Person
        {
            Id = dbPerson.Id,
            FirstName = dbPerson.FirstName,
            LastName = dbPerson.LastName,
            Email = dbPerson.Email,
            City = dbPerson.City
        };

        return person;
    }

    public async Task<Result<PersonWithImage>> GetWithImageAsync(Guid id)
    {
        var personResult = await GetAsync(id);

        if (!personResult) // A shortcut to !personResult.Success
        {
            return Result.Fail(personResult.FailureReason);
        }

        var person = personResult.Content!;

        var imageResult = await imageService.GetImageAsync();
        if (imageResult.TryGetContent(out var imageFileContent) && imageFileContent is not null)
        {
            // The image operation succeeded, return person with image
            var personWithImage = new PersonWithImage
            {
                Person = person,
                Image = imageFileContent.Content
            };

            return personWithImage;
        }

        // The image operation failed, return person without image
        var personWithoutImage = new PersonWithImage
        {
            Person = person
        };

        return personWithoutImage;
    }

    public async Task<Result<Person>> SaveAsync(Person person)
    {
        var dbPerson = person.Id != Guid.Empty ? await dbContext.People
            .FirstOrDefaultAsync(p => p.Id == person.Id) : null;

        if (dbPerson is null)
        {
            var samePersonExists = await dbContext.People
                .AnyAsync(p => p.FirstName == person.FirstName && p.LastName == person.LastName
                && p.CreateDate.AddMinutes(1) > DateTime.UtcNow);

            if (samePersonExists)
            {
                var validationErrors = new List<ValidationError>
                {
                    new("FirstName", "First name already in use"),
                    new("LastName", "Last name already in use")
                };

                return Result.Fail(FailureReasons.ClientError, "Unable to create a person with same first name and last name within 1 minute", validationErrors);
            }

            dbPerson = new Entities.Person
            {
                CreateDate = DateTime.UtcNow
            };

            dbContext.Add(dbPerson);
        }

        dbPerson.FirstName = person.FirstName;
        dbPerson.LastName = person.LastName;
        dbPerson.Email = person.Email;
        dbPerson.City = person.City;

        await dbContext.SaveChangesAsync();
        person.Id = dbPerson.Id;

        return person;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var dbPerson = await dbContext.People.FirstOrDefaultAsync(p => p.Id == id);

        if (dbPerson is not null)
        {
            if (dbPerson.FirstName == "Admin")
            {
                return Result.Fail(FailureReasons.Forbidden, "You cannot delete the Admin user");
            }

            dbContext.Remove(dbPerson);
            await dbContext.SaveChangesAsync();

            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ItemNotFound);
    }
}
