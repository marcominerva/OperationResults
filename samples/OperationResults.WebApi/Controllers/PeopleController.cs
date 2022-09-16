using Microsoft.AspNetCore.Mvc;
using OperationResults.AspNetCore;
using OperationResults.WebApi.Services;

namespace OperationResults.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly PeopleService peopleService;

    public PeopleController(PeopleService peopleService)
    {
        this.peopleService = peopleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery(Name = "q")] string queryText)
    {
        var result = await peopleService.GetAsync(queryText);

        var response = HttpContext.CreateResponse(result);
        return response;
    }
}
