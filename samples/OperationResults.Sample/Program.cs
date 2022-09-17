using Microsoft.EntityFrameworkCore;
using OperationResults.AspNetCore;
using OperationResults.Sample.BusinessLayer;
using OperationResults.Sample.BusinessLayer.Services;
using OperationResults.Sample.BusinessLayer.Services.Interfaces;
using OperationResults.Sample.DataAccessLayer;

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
},
updateModelStateResponseFactory: true,
validationErrorDefaultMessage: "Errors occurred");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
