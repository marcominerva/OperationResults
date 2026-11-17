using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OperationResults.AspNetCore;

namespace OperationResults.AspNetCoreTests;

public class ControllerResponseTests
{
    private const string RequestPath = "/api/people";

    [Theory]
    [InlineData(null, StatusCodes.Status204NoContent)]
    [InlineData(StatusCodes.Status202Accepted, StatusCodes.Status202Accepted)]
    public void CreateResponse_SuccessfulResult_ReturnsStatusCode(int? successStatusCode, int expectedStatusCode)
    {
        var httpContext = CreateHttpContext();

        var response = httpContext.CreateResponse(Result.Ok(), successStatusCode);

        var statusCodeResult = Assert.IsType<StatusCodeResult>(response);
        Assert.Equal(expectedStatusCode, statusCodeResult.StatusCode);
    }

    [Fact]
    public void CreateResponse_SuccessfulResultWithRoute_ReturnsCreatedAtRoute()
    {
        var httpContext = CreateHttpContext();
        var routeValues = new { id = 42 };

        var response = httpContext.CreateResponse(Result.Ok(), "GetPerson", routeValues);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(response);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal("GetPerson", createdResult.RouteName);
        Assert.Equal(42, createdResult.RouteValues?["id"]);
        Assert.Null(createdResult.Value);
    }

    [Theory]
    [InlineData(null, StatusCodes.Status200OK)]
    [InlineData(StatusCodes.Status202Accepted, StatusCodes.Status202Accepted)]
    public void CreateResponse_SuccessfulGenericResult_ReturnsContent(int? successStatusCode, int expectedStatusCode)
    {
        var httpContext = CreateHttpContext();
        var person = new Person(Guid.NewGuid(), "Marco");

        var response = httpContext.CreateResponse<Person>(person, successStatusCode);

        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
        Assert.Same(person, objectResult.Value);
    }

    [Fact]
    public void CreateResponse_SuccessfulGenericResultWithDefaultValue_ReturnsContent()
    {
        var httpContext = CreateHttpContext();

        var response = httpContext.CreateResponse<int>(0);

        var objectResult = Assert.IsType<ObjectResult>(response);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Equal(0, objectResult.Value);
    }

    [Fact]
    public void CreateResponse_SuccessfulGenericResultWithNullContent_ReturnsNoContent()
    {
        var httpContext = CreateHttpContext();

        var response = httpContext.CreateResponse(new Result<Person?>((Person?)null));

        var statusCodeResult = Assert.IsType<StatusCodeResult>(response);
        Assert.Equal(StatusCodes.Status204NoContent, statusCodeResult.StatusCode);
    }

    [Fact]
    public void CreateResponse_SuccessfulGenericResultWithRoute_ReturnsCreatedAtRouteWithContent()
    {
        var httpContext = CreateHttpContext();
        var person = new Person(Guid.NewGuid(), "Marco");

        var response = httpContext.CreateResponse<Person>(person, "GetPerson", new { id = person.Id });

        var createdResult = Assert.IsType<CreatedAtRouteResult>(response);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal("GetPerson", createdResult.RouteName);
        Assert.Equal(person.Id, createdResult.RouteValues?["id"]);
        Assert.Same(person, createdResult.Value);
    }

    [Fact]
    public void CreateResponse_StreamFileContent_ReturnsFileStreamResult()
    {
        var httpContext = CreateHttpContext();
        using var stream = new MemoryStream([1, 2, 3]);

        var response = httpContext.CreateResponse<StreamFileContent>(new StreamFileContent(stream, "image/png", "image.png"));

        var fileResult = Assert.IsType<FileStreamResult>(response);
        Assert.Same(stream, fileResult.FileStream);
        Assert.Equal("image/png", fileResult.ContentType);
        Assert.Equal("image.png", fileResult.FileDownloadName);
    }

    [Fact]
    public void CreateResponse_ByteArrayFileContent_ReturnsFileContentResult()
    {
        var httpContext = CreateHttpContext();
        byte[] content = [1, 2, 3];

        var response = httpContext.CreateResponse<ByteArrayFileContent>(new ByteArrayFileContent(content, "image/jpg", "image.jpg"));

        var fileResult = Assert.IsType<FileContentResult>(response);
        Assert.Same(content, fileResult.FileContents);
        Assert.Equal("image/jpg", fileResult.ContentType);
        Assert.Equal("image.jpg", fileResult.FileDownloadName);
    }

    [Theory]
    [InlineData(FailureReasons.ClientError, StatusCodes.Status400BadRequest)]
    [InlineData(FailureReasons.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(FailureReasons.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(FailureReasons.ItemNotFound, StatusCodes.Status404NotFound)]
    [InlineData(FailureReasons.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(FailureReasons.InvalidContent, StatusCodes.Status422UnprocessableEntity)]
    [InlineData(FailureReasons.DatabaseError, StatusCodes.Status500InternalServerError)]
    [InlineData(FailureReasons.ServiceUnavailable, StatusCodes.Status503ServiceUnavailable)]
    public void CreateResponse_FailedResult_ReturnsMappedProblemDetails(int failureReason, int expectedStatusCode)
    {
        var httpContext = CreateHttpContext();

        var response = httpContext.CreateResponse(Result.Fail(failureReason));

        var problemDetails = AssertProblem(response, expectedStatusCode);
        Assert.Equal(ReasonPhrasesFor(expectedStatusCode), problemDetails.Title);
        Assert.Null(problemDetails.Detail);
        Assert.Equal(RequestPath, problemDetails.Instance);
        Assert.False(problemDetails.Extensions.ContainsKey("errors"));
    }

    [Fact]
    public void CreateResponse_FailedResultWithMessageAndDetail_UsesThemInProblemDetails()
    {
        var httpContext = CreateHttpContext();

        var response = httpContext.CreateResponse(Result<Person>.Fail(FailureReasons.Conflict, "message", "detail"));

        var problemDetails = AssertProblem(response, StatusCodes.Status409Conflict);
        Assert.Equal("message", problemDetails.Title);
        Assert.Equal("detail", problemDetails.Detail);
    }

    [Fact]
    public void CreateResponse_FailedResultWithException_UsesExceptionMessages()
    {
        var httpContext = CreateHttpContext();
        var exception = new InvalidOperationException("message", new Exception("detail"));

        var response = httpContext.CreateResponse(Result.Fail(FailureReasons.DatabaseError, exception));

        var problemDetails = AssertProblem(response, StatusCodes.Status500InternalServerError);
        Assert.Equal("message", problemDetails.Title);
        Assert.Equal("detail", problemDetails.Detail);
    }

    [Fact]
    public void CreateResponse_GenericResultFromNonGenericFailure_ReturnsMappedProblemDetails()
    {
        var httpContext = CreateHttpContext();
        Result<Person> result = Result.Fail(FailureReasons.ItemNotFound);

        var response = httpContext.CreateResponse(result, "GetPerson", new { id = 1 });

        AssertProblem(response, StatusCodes.Status404NotFound);
    }

    [Fact]
    public void CreateResponse_FailedResultWithRoute_ReturnsProblemDetails()
    {
        var httpContext = CreateHttpContext();

        var response = httpContext.CreateResponse(Result.Fail(FailureReasons.Forbidden), "GetPerson", new { id = 1 });

        AssertProblem(response, StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void CreateResponse_ValidationErrorsWithDefaultFormat_GroupsMessagesByName()
    {
        var httpContext = CreateHttpContext();
        ValidationError[] validationErrors = [new("FirstName", "Required"), new("FirstName", "Too short"), new("LastName", "Required")];

        var response = httpContext.CreateResponse(Result.Fail(FailureReasons.ClientError, "Invalid", validationErrors));

        var problemDetails = AssertProblem(response, StatusCodes.Status400BadRequest);
        var errors = Assert.IsType<Dictionary<string, IEnumerable<string>>>(problemDetails.Extensions["errors"]);
        Assert.Equal(["Required", "Too short"], errors["FirstName"]);
        Assert.Equal(["Required"], errors["LastName"]);
    }

    [Fact]
    public void CreateResponse_ValidationErrorsWithListFormat_ReturnsValidationErrors()
    {
        var httpContext = CreateHttpContext(options => options.ErrorResponseFormat = ErrorResponseFormat.List);
        ValidationError[] validationErrors = [new("FirstName", "Required"), new("LastName", "Required")];

        var response = httpContext.CreateResponse(Result.Fail(FailureReasons.ClientError, validationErrors));

        var problemDetails = AssertProblem(response, StatusCodes.Status400BadRequest);
        Assert.Same(validationErrors, problemDetails.Extensions["errors"]);
    }

    [Fact]
    public void CreateResponse_EmptyValidationErrors_DoesNotAddErrors()
    {
        var httpContext = CreateHttpContext();

        var response = httpContext.CreateResponse(Result.Fail(FailureReasons.ClientError, []));

        var problemDetails = AssertProblem(response, StatusCodes.Status400BadRequest);
        Assert.False(problemDetails.Extensions.ContainsKey("errors"));
    }

    [Fact]
    public void CreateResponse_UnmappedFailureReason_UsesDefaultStatusCode()
    {
        var httpContext = CreateHttpContext();

        var response = httpContext.CreateResponse(Result.Fail(12345));

        AssertProblem(response, StatusCodes.Status501NotImplemented);
    }

    [Fact]
    public void CreateResponse_StatusCodeWithoutDefaultType_UsesFallbackType()
    {
        var httpContext = CreateHttpContext(options => options.MapStatusCodes = false);

        var response = httpContext.CreateResponse(Result.Fail(499));

        var problemDetails = AssertProblem(response, 499);
        Assert.Equal("https://httpstatuses.io/499", problemDetails.Type);
    }

    [Fact]
    public void CreateResponse_UnmappedFailureReasonWithCustomDefault_UsesConfiguredStatusCode()
    {
        var httpContext = CreateHttpContext(options => options.UnmappedFailureReasonStatusCode = StatusCodes.Status503ServiceUnavailable);

        var response = httpContext.CreateResponse(Result.Fail(12345));

        AssertProblem(response, StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public void CreateResponse_UnmappedFailureReasonWithUseFailureReason_UsesFailureReasonAsStatusCode()
    {
        var httpContext = CreateHttpContext(options => options.UnmappedFailureReasonBehavior = UnmappedFailureReasonBehavior.UseFailureReason);

        var response = httpContext.CreateResponse(Result.Fail(StatusCodes.Status418ImATeapot));

        AssertProblem(response, StatusCodes.Status418ImATeapot);
    }

    [Fact]
    public void CreateResponse_CustomMapping_UsesConfiguredStatusCode()
    {
        const int customFailureReason = 42;
        var httpContext = CreateHttpContext(options => options.StatusCodesMapping.Add(customFailureReason, StatusCodes.Status423Locked));

        var response = httpContext.CreateResponse(Result.Fail(customFailureReason));

        AssertProblem(response, StatusCodes.Status423Locked);
    }

    [Fact]
    public void CreateResponse_MapStatusCodesDisabled_UsesFailureReasonAsStatusCode()
    {
        var httpContext = CreateHttpContext(options => options.MapStatusCodes = false);

        var response = httpContext.CreateResponse(Result.Fail(StatusCodes.Status409Conflict));

        AssertProblem(response, StatusCodes.Status409Conflict);
    }

    [Fact]
    public void CreateResponse_FailedResultWithoutError_ReturnsGenericError()
    {
        var httpContext = CreateHttpContext();

        AssertProblem(httpContext.CreateResponse(default), StatusCodes.Status500InternalServerError);
        AssertProblem(httpContext.CreateResponse(new Result(false)), StatusCodes.Status500InternalServerError);
        AssertProblem(httpContext.CreateResponse(default(Result<Person>)), StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void CreateResponse_FailedResultWithoutErrorAndMapStatusCodesDisabled_ReturnsGenericError()
    {
        var httpContext = CreateHttpContext(options => options.MapStatusCodes = false);

        AssertProblem(httpContext.CreateResponse(default), StatusCodes.Status500InternalServerError);
        AssertProblem(httpContext.CreateResponse(new Result(false)), StatusCodes.Status500InternalServerError);
        AssertProblem(httpContext.CreateResponse(default(Result<Person>)), StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void CreateResponse_FailedResultWithoutErrorAndMapStatusCodesDisabled_UsesGenericErrorMapping()
    {
        var httpContext = CreateHttpContext(options =>
        {
            options.MapStatusCodes = false;
            options.StatusCodesMapping[FailureReasons.GenericError] = StatusCodes.Status503ServiceUnavailable;
        });

        var response = httpContext.CreateResponse(new Result(false));

        AssertProblem(response, StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public void ToResponse_OptionsNotRegistered_UsesDefaultMapping()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();
        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };

        var response = Result.Fail(FailureReasons.ItemNotFound).ToResponse(httpContext);

        AssertProblem(response, StatusCodes.Status404NotFound);
    }

    [Fact]
    public void AddOperationResult_WithValidationMessage_ReplacesInvalidModelStateResponse()
    {
        var httpContext = CreateHttpContext(validationErrorDefaultMessage: "Invalid data");
        var actionContext = CreateActionContext(httpContext);

        var response = InvokeInvalidModelStateResponseFactory(httpContext, actionContext);

        var problemDetails = AssertProblem(response, StatusCodes.Status400BadRequest);
        Assert.Equal("Invalid data", problemDetails.Title);
        var errors = Assert.IsType<Dictionary<string, IEnumerable<string>>>(problemDetails.Extensions["errors"]);
        Assert.Equal(["Required", "Too short"], errors["FirstName"]);
        Assert.False(errors.ContainsKey("LastName"));
    }

    [Fact]
    public void AddOperationResult_WithListFormatAndMessageProvider_ReturnsValidationErrorList()
    {
        var services = CreateServices();
        services.AddOperationResult(options => options.ErrorResponseFormat = ErrorResponseFormat.List, modelState => $"{modelState.ErrorCount} errors");
        var httpContext = CreateHttpContext(services);
        var actionContext = CreateActionContext(httpContext);

        var response = InvokeInvalidModelStateResponseFactory(httpContext, actionContext);

        var problemDetails = AssertProblem(response, StatusCodes.Status400BadRequest);
        Assert.Equal("2 errors", problemDetails.Title);
        var errors = Assert.IsAssignableFrom<IEnumerable<ValidationError>>(problemDetails.Extensions["errors"]);
        Assert.Equal([new("FirstName", "Required"), new("FirstName", "Too short")], errors);
    }

    [Fact]
    public void AddOperationResult_WithoutModelStateOption_KeepsDefaultInvalidModelStateResponse()
    {
        var httpContext = CreateHttpContext();
        var actionContext = CreateActionContext(httpContext);

        var response = InvokeInvalidModelStateResponseFactory(httpContext, actionContext);

        Assert.IsNotType<JsonResult>(response);
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();

        return services;
    }

    private static DefaultHttpContext CreateHttpContext(Action<OperationResultOptions>? configuration = null)
    {
        var services = CreateServices();
        services.AddOperationResult(configuration);

        return CreateHttpContext(services);
    }

    private static DefaultHttpContext CreateHttpContext(string validationErrorDefaultMessage)
    {
        var services = CreateServices();
        services.AddOperationResult(validationErrorDefaultMessage);

        return CreateHttpContext(services);
    }

    private static DefaultHttpContext CreateHttpContext(ServiceCollection services)
    {
        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        httpContext.Request.Path = RequestPath;

        return httpContext;
    }

    private static ActionContext CreateActionContext(HttpContext httpContext)
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("FirstName", "Required");
        modelState.AddModelError("FirstName", "Too short");
        modelState.SetModelValue("LastName", "Rossi", "Rossi");

        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), modelState);
    }

    private static IActionResult InvokeInvalidModelStateResponseFactory(HttpContext httpContext, ActionContext actionContext)
    {
        var options = httpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
        return options.InvalidModelStateResponseFactory(actionContext);
    }

    private static ProblemDetails AssertProblem(IActionResult response, int expectedStatusCode)
    {
        var jsonResult = Assert.IsType<JsonResult>(response);
        Assert.Equal(expectedStatusCode, jsonResult.StatusCode);
        Assert.Equal("application/problem+json", jsonResult.ContentType);

        var problemDetails = Assert.IsType<ProblemDetails>(jsonResult.Value, exactMatch: false);
        Assert.Equal(expectedStatusCode, problemDetails.Status);
        Assert.NotNull(problemDetails.Type);

        return problemDetails;
    }

    private static string? ReasonPhrasesFor(int statusCode)
        => Microsoft.AspNetCore.WebUtilities.ReasonPhrases.GetReasonPhrase(statusCode);

    private sealed record class Person(Guid Id, string Name);
}
