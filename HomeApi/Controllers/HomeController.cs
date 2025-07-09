using System.Globalization;
using System.Text.Json;
using HomeApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HomeApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    
    // get configuration from appsettings.json
    private readonly ApiConfiguration _apiConfiguration;
    
    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IOptions<ApiConfiguration> apiConfiguration)
    {
        _logger = logger;
        _apiConfiguration = apiConfiguration.Value;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet(Name = "GetHome")]
    public async Task<Home> Get()
    {
        var client = _httpClientFactory.CreateClient();
        var cordinates = await GetCoordinatesAsync(_apiConfiguration.DefaultCity);
        var auroraJson = await client.GetStringAsync($"http://api.auroras.live/v1/?type=all&lat={cordinates.Value.Latitude}&long={cordinates.Value.Longitude}&forecast=false&threeday=false");
        var auroraForecast = JsonSerializer.Deserialize<AuroraForecast>(auroraJson);
        
        var url =
            $"{_apiConfiguration.BaseUrls.Weather}/forecast.json?key={_apiConfiguration.Keys.Weather}&q={cordinates.Value.Latitude},{cordinates.Value.Longitude}&days=7&lang=sv&aqi=yes&alerts=yes";
        var response = await client.GetAsync(url);
        
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var weather = JsonSerializer.Deserialize<WeatherData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var forecasts = weather.Forecast.Forecastday.Select(day => new Models.Forecast
        {
            Date = day.Date,
            MaxTempC = day.Day.Maxtemp_C,
            MinTempC = day.Day.Mintemp_C,
            DayIcon = day.Day.Condition.Icon,
            Astro = new Models.Astro {
                Moon_Illumination = day.Astro.Moon_Illumination,
                Moon_Phase = day.Astro.Moon_Phase,
                Moonrise = day.Astro.Moonrise,
                Moonset = day.Astro.Moonset,
                Sunrise = day.Astro.Sunrise,
                Sunset = day.Astro.Sunset
            },
            Day = MapSummary(day.Hour.Where(h => h.Is_Day == 1)),
            Night = MapSummary(day.Hour.Where(h => h.Is_Day == 0))   
        }).ToList();

        var result = new Home
        {
            CityName = cordinates.Value.name,
            Current = new Models.Current
            {
                Date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                Feelslike = weather.Current.Feelslike_C,
                IsDay = weather.Current.Is_Day,
                WindPerHour = weather.Current.Wind_Kph,
                WindGustPerHour = weather.Current.Gust_Kph,
                Temperature = weather.Current.Temp_C,
                AuroraProbability = new Probability
                {
                    Date = auroraForecast.Date,
                    Calculated = new Models.CalculatedProbability
                    {
                        Value = auroraForecast.Probability.Calculated.Value,
                        Colour = auroraForecast.Probability.Calculated.Colour,
                        Lat = auroraForecast.Probability.Calculated.Lat,
                        Long = auroraForecast.Probability.Calculated.Long
                    },
                    Colour = auroraForecast.Probability.Colour,
                    Value = auroraForecast.Probability.Value,
                    HighestProbability = new Highest
                    {
                        Colour = auroraForecast.Probability.Highest.Colour,
                        Lat = auroraForecast.Probability.Highest.Lat,
                        Long = auroraForecast.Probability.Highest.Long,
                        Value = auroraForecast.Probability.Highest.Value,
                        Date = auroraForecast.Probability.Highest.Date
                    }
                },
            },
            Forecast = forecasts,
        };
        return result;
    }

    private WeatherSummary MapSummary(IEnumerable<Hour> hours)
    {
        if (!hours.Any())
            return null;

        return new WeatherSummary
        {
            ConditionText = hours.GroupBy(h => h.Condition.Text).OrderByDescending(g => g.Count()).First().Key,
            ConditionIcon = hours.GroupBy(h => h.Condition.Icon).OrderByDescending(g => g.Count()).First().Key,
            AvgTempC = Math.Round(hours.Average(h => h.Temp_C), 1),
            AvgFeelslikeC = Math.Round(hours.Average(h => h.Feelslike_C), 1),
            TotalChanceOfRain = (int)Math.Round(hours.Average(h => h.Chance_Of_Rain)),
            TotalChanceOfSnow = (int)Math.Round(hours.Average(h => h.Chance_Of_Snow))
        };
    }
    
    public async Task<(double Latitude, double Longitude, string name)?> GetCoordinatesAsync(string address)
    {
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Add("User-Agent", "dotnet-geocoder-app");

        var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var results = JsonSerializer.Deserialize<NominatimResult[]>(content, options);

        if (!(results?.Length > 0)) 
            return null;
        
        var lat = double.Parse(results[0].Lat);
        var lon = double.Parse(results[0].Lon);
        var name = results[0].name;
        return (lat, lon, name);

    }

    private class NominatimResult
    {
        public string Lat { get; set; }
        public string Lon { get; set; }
        public string name { get; set; }
    }
}