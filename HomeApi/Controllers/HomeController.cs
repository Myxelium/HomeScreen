using HomeApi.Handlers;
using HomeApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetHome")]
    public async Task<ActionResult<WeatherInformation>> Get()
    {
        return Ok(await mediator.Send(new Weather.Command()));
    }
    
    [HttpGet("default.bmp")]
    public async Task<IActionResult> GetImage()
    {
        return File(await mediator.Send(new ImageGeneration.Command()), "image/bmp");
    }
    
    /*[HttpGet("screen/buffers")]
    public async Task<IActionResult> GetCombinedBuffers()
    {
        var (black, red) = await mediator.Send(new ImageGeneration.Command());

        // Combine buffers
        byte[] combined = new byte[black.Length + red.Length];
        Buffer.BlockCopy(black, 0, combined, 0, black.Length);
        Buffer.BlockCopy(red, 0, combined, black.Length, red.Length);

        return File(combined, "application/octet-stream");
    }*/
    
    [HttpGet("departureboard")]
    public async Task<ActionResult<List<TimeTable>>> GetDepartureBoard()
    {
        return Ok(await mediator.Send(new DepartureBoard.Command()));
    }
}