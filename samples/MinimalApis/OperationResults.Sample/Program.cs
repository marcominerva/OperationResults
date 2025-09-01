using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OperationResults.AspNetCore.Http;
using OperationResults.Sample.BusinessLayer;
using OperationResults.Sample.BusinessLayer.Services;
using OperationResults.Sample.BusinessLayer.Services.Interfaces;
using OperationResults.Sample.DataAccessLayer;
using OperationResults.Sample.Shared.Models;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("ApplicationDatabase");
});

builder.Services.AddScoped<IPeopleService, PeopleService>();
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddOperationResult(options =>
{
    options.ErrorResponseFormat = ErrorResponseFormat.Default;

    // Adds a custom mapping between CustomFailureReasons.NotAvailable (1001) and the 501 HTTP Status Code.
    // Adding new mappings or editing the existing ones allows to define what HTTP Status Codes the API must return
    // for the Operation Results of our Business Logic methods.
    options.StatusCodesMapping.Add(CustomFailureReasons.NotAvailable, StatusCodes.Status501NotImplemented);

    // If you just want to directly use HTTP status codes as failure reasons, set the following property to false.
    // In this way, the code you use with Result.Fail() will be used as response status code with no further mapping.
    //options.MapStatusCodes = false;
});

builder.Services.AddRequestLocalization("en", "it");

builder.Services.AddDefaultProblemDetails();
builder.Services.AddDefaultExceptionHandler();

builder.Services.AddOpenApi(options =>
{
    options.AddAcceptLanguageHeader();
    options.AddDefaultProblemDetailsResponse();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", app.Environment.ApplicationName);
});

app.UseRequestLocalization();

var peopleApi = app.MapGroup("api/people");

peopleApi.MapGet("/", async (IPeopleService peopleService, HttpContext httpContext) =>
{
    // You can collapse the following instructions into a single one.
    var result = await peopleService.GetAsync();

    var response = httpContext.CreateResponse(result);  // Or result.ToResponse(httpContext)
    return response;
})
.Produces<IEnumerable<Person>>()
.WithOpenApi();

peopleApi.MapGet("{id:guid}", async (Guid id, IPeopleService peopleService, HttpContext httpContext) =>
{
    // You can collapse the following instructions into a single one.
    var result = await peopleService.GetAsync(id);

    var response = httpContext.CreateResponse(result);  // Or result.ToResponse(httpContext)
    return response;
})
.Produces<Person>()
.Produces(StatusCodes.Status404NotFound)
.WithName("GetPerson")
.WithOpenApi();

peopleApi.MapGet("{id:guid}/with-image", async (Guid id, IPeopleService peopleService, HttpContext httpContext) =>
{
    // You can collapse the following instructions into a single one.
    var result = await peopleService.GetWithImageAsync(id);

    var response = httpContext.CreateResponse(result);  // Or result.ToResponse(httpContext)
    return response;
})
.Produces<PersonWithImage>()
.Produces(StatusCodes.Status404NotFound)
.WithName("GetPersonWithImage")
.WithOpenApi();

peopleApi.MapPost("/", async (Person person, IPeopleService peopleService, HttpContext httpContext) =>
{
    // You can collapse the following instructions into a single one.
    var result = await peopleService.SaveAsync(person);

    var response = httpContext.CreateResponse(result, "GetPerson", new { id = result.Content?.Id });  // Or result.ToResponse(httpContext, nameof(GetPerson), new { id = result.Content?.Id });
    return response;
})
.Produces<Person>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)
.WithOpenApi();

peopleApi.MapDelete("{id:guid}", async (Guid id, IPeopleService peopleService, HttpContext httpContext) =>
{
    // You can collapse the following instructions into a single one.
    var result = await peopleService.DeleteAsync(id);

    var response = httpContext.CreateResponse(result);  // Or result.ToResponse(httpContext)
    return response;
})
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.WithOpenApi();

app.MapGet("api/image", async (IImageService imageService, HttpContext httpContext)
    => httpContext.CreateResponse(await imageService.GetImageAsync())
)
.Produces<FileContentResult>(StatusCodes.Status200OK, contentType: MediaTypeNames.Application.Octet)
.Produces(StatusCodes.Status404NotFound)
.WithOpenApi();

app.Run();