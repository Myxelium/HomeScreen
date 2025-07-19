using HomeApi.Handlers;
using HomeApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeApi.Controllers;

[ApiController]
[Route("home")]
public class HomeController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "getHome")]
    public async Task<ActionResult<WeatherInformation>> Get()
    {
        return Ok(await mediator.Send(new Weather.Command()));
    }
    
    [HttpGet("default.jpg")]
    public async Task<IActionResult> GetImage()
    {
        return File(await mediator.Send(new ImageGeneration.Command()), "image/jpeg");
    }
    
    [HttpGet("configuration")]
    public async Task<ActionResult<MicroProcessorConfiguration>> GetCombinedBuffers()
    {
        return Ok(await mediator.Send(new Configuration.Command()));
    }
    
    [HttpGet("departure-board")]
    public async Task<ActionResult<List<TimeTable>>> GetDepartureBoard()
    {
        return Ok(await mediator.Send(new DepartureBoard.Command()));
    }
}