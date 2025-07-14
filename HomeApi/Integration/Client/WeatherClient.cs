using HomeApi.Models.Response;

namespace HomeApi.Integration.Client;

using Refit;

public interface IWeatherClient
{
    [Get("/forecast.json")]
    Task<WeatherData> GetForecastAsync(
        [AliasAs("key")] string apiKey,
        [AliasAs("q")] string coordinates,
        [AliasAs("days")] int days = 7,
        [AliasAs("lang")] string language = "sv",
        [AliasAs("aqi")] string aqi = "yes",
        [AliasAs("alerts")] string alerts = "yes");
}
