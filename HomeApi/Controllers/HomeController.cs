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
        var result = await mediator.Send(new GetWeather.Command());
        return Ok(result);
    }
}