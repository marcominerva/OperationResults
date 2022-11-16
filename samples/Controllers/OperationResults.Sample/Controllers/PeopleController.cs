using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OperationResults.AspNetCore;
using OperationResults.Sample.BusinessLayer.Services.Interfaces;
using OperationResults.Sample.Shared.Models;

namespace OperationResults.Sample.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class PeopleController : ControllerBase
{
    private readonly IPeopleService peopleService;

    public PeopleController(IPeopleService peopleService)
    {
        this.peopleService = peopleService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Person>))]
    public async Task<IActionResult> GetList()
    {
        // You can collapse the following instructions into a single one.
        var result = await peopleService.GetAsync();

        var response = HttpContext.CreateResponse(result);  // Or result.ToResponse(HttpContext)
        return response;
    }

    [HttpGet("{id:guid}", Name = nameof(GetPerson))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Person))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPerson(Guid id)
    {
        // You can collapse the following instructions into a single one.
        var result = await peopleService.GetAsync(id);

        var response = HttpContext.CreateResponse(result);  // Or result.ToResponse(HttpContext)
        return response;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Person))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Save(Person person)
    {
        // You can collapse the following instructions into a single one.
        var result = await peopleService.SaveAsync(person);

        var response = HttpContext.CreateResponse(result, nameof(GetPerson), new { id = result.Content?.Id });  // Or result.ToResponse(HttpContext, nameof(GetPerson), new { id = result.Content?.Id });
        return response;
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        // You can collapse the following instructions into a single one.
        var result = await peopleService.DeleteAsync(id);

        var response = HttpContext.CreateResponse(result);  // Or result.ToResponse(HttpContext)
        return response;
    }
}
