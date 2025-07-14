using HomeApi.Integration.Client;
using HomeApi.Models.Response;

namespace HomeApi.Integration;

public interface IGeocodingService
{
    Task<NomatimApiResponse?> GetCoordinatesAsync(string address);
}

public class GeocodingService(INominatimClient nominatimApi) : IGeocodingService
{
    public async Task<NomatimApiResponse?> GetCoordinatesAsync(string address)
    {
        var results = await nominatimApi.SearchAsync(address);
        
        return results.FirstOrDefault();
    }
}