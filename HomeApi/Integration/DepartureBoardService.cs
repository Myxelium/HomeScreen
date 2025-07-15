using HomeApi.Extensions;
using HomeApi.Integration.Client;
using HomeApi.Models;
using HomeApi.Models.Configuration;
using Microsoft.Extensions.Options;

namespace HomeApi.Integration;

public interface IDepartureBoardService
{
    Task<List<TimeTable>?> GetDepartureBoard();
}

public class DepartureBoardService(IResRobotClient departureBoardApi, IOptions<ApiConfiguration> options) : IDepartureBoardService
{
    private readonly ApiConfiguration _apiConfig = options.Value;

    public async Task<List<TimeTable>?> GetDepartureBoard()
    {
        var locationResponse = await departureBoardApi.GetLocationsByNameAsync(
            input: _apiConfig.DefaultStation,
            format: "json",
            accessId: _apiConfig.Keys.ResRobot
        );

        var id = locationResponse.StopLocationOrCoordLocation.FirstOrDefault()?.StopLocation?.ExtId;

        if (id == null)
            return null;
        
        var result = await departureBoardApi.GetDepartureBoardAsync(
            accessId: _apiConfig.Keys.ResRobot,
            stopId: id,
            direction: null,
            date: DateTime.Now.ToString("yyyy-MM-dd"),
            time: DateTime.Now.ToString("HH:mm"),
            duration: 60,
            maxJourneys: 10,
            passlist: 1,
            language: "sv",
            format: "json"
        );
    
        return result.ToContract();
    }
}