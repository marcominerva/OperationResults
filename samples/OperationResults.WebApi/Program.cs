using OperationResults;
using OperationResults.AspNetCore;
using OperationResults.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<PeopleService>();

builder.Services.AddOperationResult(options =>
{
    options.ErrorResponseFormat = ErrorResponseFormat.Default;
    //options.StatusCodesMapping.Add(42, StatusCodes.Status406NotAcceptable);
    options.StatusCodesMapping[FailureReasons.DatabaseError] = StatusCodes.Status502BadGateway;
}, true, "Errori di validazione");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
