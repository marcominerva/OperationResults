using Microsoft.AspNetCore.Mvc;
using OperationResults.AspNetCore;
using OperationResults.Sample.BusinessLayer.Services.Interfaces;

namespace OperationResults.Sample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly IImageService imageService;

    public ImageController(IImageService imageService)
    {
        this.imageService = imageService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImage()
        => HttpContext.CreateResponse(await imageService.GetImageAsync());
}
