using HomeApi.Integration.Client;
using HomeApi.Models.Response;

namespace HomeApi.Integration;

public interface IAuroraService
{
    Task<AuroraForecastApiResponse> GetAuroraForecastAsync(string lat, string lon);
}

public class AuroraService(IAuroraClient auroraApi) : IAuroraService
{
    public Task<AuroraForecastApiResponse> GetAuroraForecastAsync(string lat, string lon)
    {
        try
        {
            return auroraApi.GetForecastAsync(latitude: lat, longitude: lon);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}