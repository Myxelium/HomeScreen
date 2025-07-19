using HomeApi.Models.Response;
using Refit;

namespace HomeApi.Integration.Client.WeatherClient;

public interface IWeatherClient
{
    [Get("/forecast.json")]
    Task<WeatherData> GetForecastAsync(
        [AliasAs("key")] string apiKey,
        [AliasAs("q")] string coordinates,
        [AliasAs("days")] int days = 14,
        [AliasAs("lang")] string language = "sv",
        [AliasAs("aqi")] string aqi = "yes",
        [AliasAs("alerts")] string alerts = "yes");
}
