using HomeApi.Extensions;
using HomeApi.Integration;
using HomeApi.Integration.Client;
using HomeApi.Integration.Client.WeatherClient;
using HomeApi.Models.Configuration;
using Microsoft.Extensions.Options;
using Refit;

namespace HomeApi.Registration;

public static class RegisterIntegration
{
    public static void AddIntegration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ApiConfiguration>(configuration.GetSection("ApiConfiguration"));

        services.AddRefitClient<INominatimClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var config = sp.GetRequiredService<IOptions<ApiConfiguration>>().Value;
                client.BaseAddress = new Uri(config.BaseUrls.Nominatim);
                client.DefaultRequestHeaders.Add("User-Agent", "dotnet-geocoder-app");
            });


        services.AddRefitClient<IAuroraClient>()
            .ConfigureBaseAddress(apiConfiguration => apiConfiguration.BaseUrls.Aurora);

        services.AddRefitClient<IWeatherClient>()
            .ConfigureBaseAddress(apiConfiguration => apiConfiguration.BaseUrls.Weather);
        
        services.AddRefitClient<IResRobotClient>()
            .ConfigureBaseAddress(apiConfiguration => apiConfiguration.BaseUrls.ResRobot);

        services.AddScoped<IDepartureBoardService, DepartureBoardService>();
        services.AddScoped<IGeocodingService, GeocodingService>();
        services.AddScoped<IAuroraService, AuroraService>();
        services.AddScoped<IWeatherService, WeatherService>();
    }
}