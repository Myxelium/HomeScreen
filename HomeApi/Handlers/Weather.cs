using HomeApi.Extensions;
using HomeApi.Integration;
using HomeApi.Models;
using HomeApi.Models.Configuration;
using MediatR;
using Microsoft.Extensions.Options;

namespace HomeApi.Handlers;

public static class Weather
{
    public record Command : IRequest<WeatherInformation>;

    public class Handler : IRequestHandler<Command, WeatherInformation>
    {
        private readonly ApiConfiguration _apiConfiguration;
        private readonly ILogger<Handler> _logger;
        private readonly IGeocodingService _geocodingService;
        private readonly IAuroraService _auroraService;
        private readonly IWeatherService _weatherService;

        public Handler(
            IOptions<ApiConfiguration> apiConfiguration,
            ILogger<Handler> logger,
            IGeocodingService geocodingService,
            IAuroraService auroraService,
            IWeatherService weatherService)
        {
            _apiConfiguration = apiConfiguration.Value;
            _logger = logger;
            _geocodingService = geocodingService;
            _auroraService = auroraService;
            _weatherService = weatherService;
        }

        public async Task<WeatherInformation> Handle(Command request, CancellationToken cancellationToken)
        {
            var coordinates = await _geocodingService.GetCoordinatesAsync(_apiConfiguration.DefaultCity);
            if (coordinates is null)
                throw new Exception("Coordinates not found");

            var aurora = await _auroraService.GetAuroraForecastAsync(coordinates.Lat, coordinates.Lon);
            var weather = await _weatherService.GetWeatherAsync(coordinates.Lat, coordinates.Lon);

            var forecasts = weather.Forecast.Forecastday.Select(day => day.ToForecast()).ToList();

            return weather.ToContract(coordinates.Name, aurora, forecasts);
        }
    }
}