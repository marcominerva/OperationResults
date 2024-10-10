using Microsoft.EntityFrameworkCore;
using OperationResults.AspNetCore;
using OperationResults.Example.BusinessLayer;
using OperationResults.Example.BusinessLayer.Services;
using OperationResults.Example.BusinessLayer.Services.Interfaces;
using OperationResults.Example.DataAccessLayer;
using OperationResults.Example.Resources;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

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
},
//updateModelStateResponseFactory: true);
// Passing a validation error default message or a validation error message provider automatically update the ModelStateResponseFactory.
//validationErrorDefaultMessage: "Errors occurred");                            // Use this to set a static default message
validationErrorMessageProvider: (state) => Messages.ValidationErrorMessage);    // Use this if you need to dinamically change the message, i.e., based on user culture.

builder.Services.AddRequestLocalization("en", "it");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddAcceptLanguageHeader();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLocalization();

app.UseAuthorization();

app.MapControllers();

app.Run();
