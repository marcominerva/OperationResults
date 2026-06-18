# OperationResults

[![Lint Code Base](https://github.com/marcominerva/OperationResults/actions/workflows/linter.yml/badge.svg)](https://github.com/marcominerva/OperationResults/actions/workflows/linter.yml)
[![CodeQL](https://github.com/marcominerva/OperationResults/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/marcominerva/OperationResults/actions/workflows/github-code-scanning/codeql)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/marcominerva/OperationResultTools/blob/master/LICENSE)

A set of lightweight libraries to totally decouple operation results and actual application responses.

## Core library

[![NuGet](https://img.shields.io/nuget/v/OperationResultTools.svg?style=flat-square)](https://www.nuget.org/packages/OperationResultTools)
[![Nuget](https://img.shields.io/nuget/dt/OperationResultTools)](https://www.nuget.org/packages/OperationResultTools)

**Installation**

The library is available on [NuGet](https://www.nuget.org/packages/OperationResultTools). Just search for *OperationResultTools* in the **Package Manager GUI** or run the following command in the **.NET CLI**:

    dotnet add package OperationResultTools

**Usage example**

The core library can be used in your business layer to return successful or failed results without referencing ASP.NET Core types.

```csharp
public class PeopleService(ApplicationDbContext dbContext, IImageService imageService) : IPeopleService
{
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

    public async Task<Result<Person>> SaveAsync(Person person)
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

            return Result.Fail(
                FailureReasons.ClientError,
                "Unable to create a person with same first name and last name within 1 minute",
                validationErrors);
        }

        return person;
    }
}
```

`Result<T>` can be returned directly from a value when the operation succeeds, and `Result.Fail` can be used when the operation needs to carry a failure reason, a message, and optional validation errors. This keeps application services focused on business outcomes instead of HTTP response types.

You can also inspect a result without throwing exceptions:

```csharp
var result = await peopleService.GetAsync(id);

if (result.TryGetContent(out var person))
{
    Console.WriteLine($"Loaded {person.FirstName} {person.LastName}");
}
else if (result.HasError)
{
    Console.WriteLine(result.ErrorMessage);
}
```

The same pattern also works for file results or other payloads:

```csharp
public class ImageService : IImageService
{
    public async Task<Result<ByteArrayFileContent>> GetImageAsync()
    {
        if (!File.Exists(@"D:\Taggia.jpg"))
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        var content = await File.ReadAllBytesAsync(@"D:\Taggia.jpg");
        return new ByteArrayFileContent(content, "image/jpg");
    }
}
```

## ASP.NET Core integration library (Controller-based projects)

[![NuGet](https://img.shields.io/nuget/v/OperationResultTools.AspNetCore.svg?style=flat-square)](https://www.nuget.org/packages/OperationResultTools.AspNetCore)
[![Nuget](https://img.shields.io/nuget/dt/OperationResultTools.AspNetCore)](https://www.nuget.org/packages/OperationResultTools.AspNetCore)

_Note: This is the library to use if you're working with Controller._

This library provides HttpContext extension methods to automatically map Operation Results (that may come, for sample, from a business layer) to HTTP responses, along with the appropriate status codes.

A full sample is available in the [Sample folder](https://github.com/marcominerva/OperationResults/tree/master/samples). Search for the registration in the [Program.cs](https://github.com/marcominerva/OperationResults/blob/master/samples/Controllers/OperationResults.Sample/Program.cs#L24-L40) file and the usage in [Controllers folder](https://github.com/marcominerva/OperationResults/tree/master/samples/Controllers/OperationResults.Sample/Controllers).

**Installation**

The library is available on [NuGet](https://www.nuget.org/packages/OperationResultTools.AspNetCore). Just search for *OperationResultTools.AspNetCore* in the **Package Manager GUI** or run the following command in the **.NET CLI**:

    dotnet add package OperationResultTools.AspNetCore

**Usage example**

Once the package is registered, your controllers can delegate the HTTP response generation to `HttpContext.CreateResponse`.

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class PeopleController(IPeopleService peopleService) : ControllerBase
{
    [HttpGet("{id:guid}", Name = nameof(GetPerson))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Person))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPerson(Guid id)
    {
        var result = await peopleService.GetAsync(id);
        return HttpContext.CreateResponse(result);
    }

    [HttpPost]
    [ProducesResponseType<Person>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Save(Person person)
    {
        var result = await peopleService.SaveAsync(person);
        return HttpContext.CreateResponse(result, nameof(GetPerson), new { id = result.Content?.Id });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await peopleService.DeleteAsync(id);
        return HttpContext.CreateResponse(result);
    }
}
```

This approach also works for binary responses:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ImageController(IImageService imageService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImage()
        => HttpContext.CreateResponse(await imageService.GetImageAsync());
}
```

**Configuration**

Register the controller adapter once at startup. You can keep the default mapping between `FailureReasons` and HTTP status codes, add custom failure reasons, or decide to use failure reasons directly as HTTP status codes:

```csharp
builder.Services.AddOperationResult(options =>
{
    options.ErrorResponseFormat = ErrorResponseFormat.Default;
    options.StatusCodesMapping.Add(CustomFailureReasons.NotAvailable, StatusCodes.Status501NotImplemented);

    // If your application uses HTTP status codes directly as failure reasons:
    // options.MapStatusCodes = false;

    // If a failure reason is not mapped, use a fallback status code.
    options.UnmappedFailureReasonBehavior = UnmappedFailureReasonBehavior.UseDefaultStatusCode;
    options.UnmappedFailureReasonStatusCode = StatusCodes.Status501NotImplemented;
});
```

Controller-based projects can also opt in to operation-result-style automatic model-state validation responses:

```csharp
builder.Services.AddOperationResult(
    options => options.ErrorResponseFormat = ErrorResponseFormat.List,
    validationErrorDefaultMessage: "The submitted data is invalid");
```

When this option is used, invalid model-state responses are emitted as `application/problem+json` and reuse the configured validation error format.

## ASP.NET Core integration library (Minimal API projects)

[![NuGet](https://img.shields.io/nuget/v/OperationResultTools.AspNetCore.Http.svg?style=flat-square)](https://www.nuget.org/packages/OperationResultTools.AspNetCore.Http)
[![Nuget](https://img.shields.io/nuget/dt/OperationResultTools.AspNetCore.Http)](https://www.nuget.org/packages/OperationResultTools.AspNetCore.Http)

_Note: This is the library to use if you're working with Minimal APIs._

This library provides HttpContext extension methods to automatically map Operation Results (that may come, for sample, from a business layer) to HTTP responses, along with the appropriate status codes.

A full sample is available in the [Samples folder](https://github.com/marcominerva/OperationResults/tree/master/samples). Search for the [registration](https://github.com/marcominerva/OperationResults/blob/master/samples/MinimalApis/OperationResults.Sample/Program.cs#L23-L35) and the [usage](https://github.com/marcominerva/OperationResults/blob/master/samples/MinimalApis/OperationResults.Sample/Program.cs#L51-L106) in [Program.cs](https://github.com/marcominerva/OperationResults/blob/master/samples/MinimalApis/OperationResults.Sample/Program.cs) file.

**Installation**

The library is available on [NuGet](https://www.nuget.org/packages/OperationResultTools.AspNetCore.Http). Just search for *OperationResultTools.AspNetCore.Http* in the **Package Manager GUI** or run the following command in the **.NET CLI**:

    dotnet add package OperationResultTools.AspNetCore.Http

**Usage example**

Register the library in your Minimal API application and customize the mapping between operation failures and HTTP status codes:

```csharp
builder.Services.AddOperationResult(options =>
{
    options.ErrorResponseFormat = ErrorResponseFormat.Default;
    options.StatusCodesMapping.Add(CustomFailureReasons.NotAvailable, StatusCodes.Status501NotImplemented);

    // If you want to use failure reasons directly as HTTP status codes:
    // options.MapStatusCodes = false;

    // If a failure reason is not mapped, use a fallback status code.
    options.UnmappedFailureReasonBehavior = UnmappedFailureReasonBehavior.UseDefaultStatusCode;
    options.UnmappedFailureReasonStatusCode = StatusCodes.Status501NotImplemented;
});
```

Then convert results returned by your services into HTTP responses inside your endpoints:

```csharp
var peopleApi = app.MapGroup("api/people");

peopleApi.MapGet("/", async (IPeopleService peopleService, HttpContext httpContext) =>
{
    var result = await peopleService.GetAsync();
    return httpContext.CreateResponse(result);
})
.Produces<IEnumerable<Person>>();

peopleApi.MapGet("{id:guid}", async (Guid id, IPeopleService peopleService, HttpContext httpContext) =>
{
    var result = await peopleService.GetAsync(id);
    return httpContext.CreateResponse(result);
})
.Produces<Person>()
.Produces(StatusCodes.Status404NotFound)
.WithName("GetPerson");

peopleApi.MapPost("/", async (Person person, IPeopleService peopleService, HttpContext httpContext) =>
{
    var result = await peopleService.SaveAsync(person);
    return httpContext.CreateResponse(result, "GetPerson", new { id = result.Content?.Id });
})
.Produces<Person>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json);
```

Minimal API endpoints can return non-generic results as well. Successful non-generic results are translated to `204 No Content` by default, while failures are translated to problem details using the configured status-code mapping:

```csharp
peopleApi.MapDelete("{id:guid}", async (Guid id, IPeopleService peopleService, HttpContext httpContext) =>
{
    var result = await peopleService.DeleteAsync(id);
    return httpContext.CreateResponse(result);
})
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status403Forbidden)
.Produces(StatusCodes.Status404NotFound);
```

If a service returns content together with a failure, the adapter returns that content with the mapped failure status code instead of creating a problem details response. This is useful for partial results or domain-specific error payloads.

The same mechanism can also return files:

```csharp
app.MapGet("api/image", async (IImageService imageService, HttpContext httpContext)
    => httpContext.CreateResponse(await imageService.GetImageAsync())
)
.Produces<FileContentResult>(StatusCodes.Status200OK, contentType: MediaTypeNames.Application.Octet)
.Produces(StatusCodes.Status404NotFound);
```

In this way, the business layer remains completely decoupled from ASP.NET Core, while the web layer consistently translates `Result` and `Result<T>` instances into HTTP responses.


**Contribute**

The project is constantly evolving. Contributions are welcome. Feel free to file issues and pull requests on the repo and we'll address them as we can.
