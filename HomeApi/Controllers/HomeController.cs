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
    
    [HttpGet("default.png")]
    public async Task<IActionResult> GetImage()
    {
        return File(await mediator.Send(new ImageGeneration.Command()), "image/png");
    }
    
    [HttpGet("departureboard")]
    public async Task<ActionResult<List<TimeTable>>> GetDepartureBoard()
    {
        return Ok(await mediator.Send(new DepartureBoard.Command()));
    }
}