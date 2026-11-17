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

> [!WARNING]
> **Compatibility note:** version 2.0 requires **.NET 11** and **C# 15**. If your project targets .NET 10 or earlier, keep using version **1.x**.
>
> Starting from version 2.0, `Result` and `Result<T>` are [custom union types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union#custom-union-types), and this introduces breaking changes. Please see the [Upgrade Guide](#upgrade-guide) for more details.

**Compatibility**

| **Version** | **.NET**       | **C#** | **Notes**                                                   |
|:------------|:---------------|:-------|:------------------------------------------------------------|
| 2.x         | 11.0 or later  | 15     | `Result` and `Result<T>` are custom union types             |
| 1.x         | 10.0, 9.0, 8.0 | -      | Use this version if your project targets .NET 10 or earlier |

The same versioning applies to all the packages (`OperationResultTools`, `OperationResultTools.AspNetCore` and `OperationResultTools.AspNetCore.Http`).

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

A non-generic `Result` converts implicitly to any `Result<T>`, so `Result.Fail(...)` can be returned from methods that return `Result<T>`. If you prefer to be explicit, `Result<T>.Fail(...)` exposes the same overloads. The conversion works as follows:

| Source `Result` | Converted `Result<T>` |
|---|---|
| `Result.Fail(...)` | Failure with the same `Error` instance |
| `Result.Ok()` | Success with `default(T)` content |
| `new Result(false)` or `default(Result)` (which is the same) | `default(Result<T>)`: not successful and without an `Error` |

A `Result<T>` is a union of `T` and `Error`, so you can inspect it with pattern matching:

```csharp
var result = await peopleService.GetAsync(id);

if (result is Person person)
{
    Console.WriteLine($"Loaded {person.FirstName} {person.LastName}");
}
else if (result is Error error)
{
    Console.WriteLine(error.ErrorMessage);
}
```

An `Error` converts implicitly to any result type, so a failure can be propagated as is:

```csharp
var personResult = await GetAsync(id);
if (personResult is Error error)
{
    return error;
}
```

The `Success` property (also available through the `if (result)` shortcut) is based on an explicit discriminator, not on the content:

| Result | `Success` | `Content` | Pattern that matches |
|---|---|---|---|
| Success with content (including `0` or `false`) | `true` | the content | `T` |
| Success with `null` content | `true` | `null` | `null` |
| Failure | `false` | throws `InvalidOperationException` | `Error` |
| `default(Result<T>)` | `false` | `default(T)` | `null` |

Because type patterns never match `null`, use `Success` when you need to tell a successful `null` content apart from an uninitialized result.

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
        return HttpContext.CreateResponse(result, nameof(GetPerson), new { id = result is Person saved ? saved.Id : default(Guid?) });
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
    return httpContext.CreateResponse(result, "GetPerson", new { id = result is Person saved ? saved.Id : default(Guid?) });
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

A failed result is always translated to a problem details response built from its `Error`. A failed result without an `Error` (for example, a `default` instance) is treated as `FailureReasons.GenericError`.

The same mechanism can also return files:

```csharp
app.MapGet("api/image", async (IImageService imageService, HttpContext httpContext)
    => httpContext.CreateResponse(await imageService.GetImageAsync())
)
.Produces<FileContentResult>(StatusCodes.Status200OK, contentType: MediaTypeNames.Application.Octet)
.Produces(StatusCodes.Status404NotFound);
```

In this way, the business layer remains completely decoupled from ASP.NET Core, while the web layer consistently translates `Result` and `Result<T>` instances into HTTP responses.

## Upgrade Guide

### From 1.x to 2.0

Version 2.0 rewrites `Result` and `Result<T>` as [custom union types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union#custom-union-types), a C# 15 feature. For this reason, **version 2.0 requires .NET 11 or later**, for both the core library and the ASP.NET Core integration libraries.

> [!IMPORTANT]
> If your project targets .NET 10 or earlier, you must stay on version 1.x, which continues to work as before:
>
> ```xml
> <PackageReference Include="OperationResultTools" Version="1.*" />
> <PackageReference Include="OperationResultTools.AspNetCore" Version="1.*" />
> <PackageReference Include="OperationResultTools.AspNetCore.Http" Version="1.*" />
> ```

To upgrade, change the target framework of your projects to `net11.0`, update all the OperationResultTools packages to 2.0, and then apply the following changes.

**`Result` and `Result<T>` are now structs**

They are `readonly struct` union types: `Result<T>` is a union of `T` and `Error`, and `Result` is a union of `bool` and `Error`. As a consequence, they can't be `null`, and `default(Result<T>)` is a valid instance that is neither a success nor a failure with an `Error`. Code that checks a result for `null` must be removed.

**Error information moved to the `Error` class**

In 1.x, `FailureReason`, `ErrorMessage`, `ErrorDetail`, `ValidationErrors`, `HasError` and the `Error` property (the exception) were exposed directly by the result. In 2.0 they belong to the `Error` case of the union, which you get with pattern matching. The exception is now available through `Error.Exception`:

```csharp
// 1.x
if (!result.Success)
{
    logger.LogError(result.Error, "Operation failed with reason {Reason}: {Message}", result.FailureReason, result.ErrorMessage);
}

// 2.0
if (result is Error error)
{
    logger.LogError(error.Exception, "Operation failed with reason {Reason}: {Message}", error.FailureReason, error.ErrorMessage);
}
```

**`Result<T>.Ok` has been removed**

Return the value directly (it is converted implicitly), or use the constructor:

```csharp
// 1.x
return Result<Person>.Ok(person);

// 2.0
return person;
// or
return new Result<Person>(person);
```

`Result.Ok()` is still available for the non-generic `Result`.

**`TryGetContent` has been replaced by pattern matching**

```csharp
// 1.x
if (result.TryGetContent(out var person))
{
    // ...
}

// 2.0
if (result is Person person)
{
    // ...
}
```

**`Content` throws on failure**

In 1.x `Content` returned `default(T)` for a failed result. In 2.0 it throws an `InvalidOperationException` when the result contains an `Error`. Use pattern matching when the result may be a failure:

```csharp
// 1.x
var id = result.Content?.Id;

// 2.0
var id = result is Person person ? person.Id : default(Guid?);
```

**`Success` is based on a discriminator**

`Success` is `true` for every successful result, even when its content is `null`, `0` or `false`. Because type patterns never match `null`, use `Success` (or the `null` pattern) to handle a successful `null` content.

**Failure content is no longer supported**

The `Fail` overloads that accepted a `T content` argument have been removed, and the ASP.NET Core integration libraries no longer write content in failure responses. A failure is always translated to a problem details response built from its `Error`. If you need to return extra data, use the message, the detail or the validation errors.

**Failures without an `Error` are mapped to `GenericError`**

A result that is not successful and has no `Error` (for example, `default(Result<T>)` or `new Result(false)`) is translated to a `FailureReasons.GenericError` response (500 Internal Server Error by default).

**Changes to `Map` and `MapPaginated`**

- `Map` and `MapPaginated` throw an `InvalidOperationException` when called on a `default` instance, because it represents neither a success nor a failure.
- `MapPaginated` on a successful result with `null` content now returns a successful result with `null` content instead of a failure.

**`PaginatedList<T>(items, totalCount, pageIndex)` has been removed**

That constructor inferred the page size from the number of supplied items, so on a partial last page it computed wrong `PageSize` and `HasNextPage` values (for example, 7 items in pages of 3: the third page contains 1 item, and the constructor reported a page size of 1 and another page available). Pass the page size explicitly:

```csharp
// 1.x
var page = new PaginatedList<Person>(people, totalCount, pageIndex);

// 2.0
var page = new PaginatedList<Person>(people, totalCount, pageIndex, pageSize);
```

The other constructors now also treat `null` items as an empty page in every case: in 1.x, `new PaginatedList<T>(null, totalCount)` always set `HasNextPage` to `false`, while an empty collection set it to `true` when `totalCount` was greater than zero.

**Contribute**

The project is constantly evolving. Contributions are welcome. Feel free to file issues and pull requests on the repo and we'll address them as we can.
