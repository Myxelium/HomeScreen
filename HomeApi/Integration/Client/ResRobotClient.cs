using HomeApi.Models.Response;
using Refit;

namespace HomeApi.Integration.Client;

public interface IResRobotClient
{
    [Get("/v2.1/departureBoard")]
    Task<TrafikLabsApiResponse> GetDepartureBoardAsync(
        [AliasAs("accessId")] string accessId,
        [AliasAs("id")] string stopId,
        [AliasAs("direction")] string direction = null,
        [AliasAs("date")] string date = null,              // Format: YYYY-MM-DD
        [AliasAs("time")] string time = null,              // Format: HH:MM
        [AliasAs("duration")] int? duration = null,
        [AliasAs("maxJourneys")] int? maxJourneys = null,
        [AliasAs("operators")] string operators = null,    // Example: "275,287"
        [AliasAs("products")] int? products = null,
        [AliasAs("passlist")] int? passlist = 0,
        [AliasAs("lang")] string language = "sv",
        [AliasAs("format")] string format = "json"
    );
    
    [Get("/v2.1/location.name")]
    Task<LocationNameResponse> GetLocationsByNameAsync(
        [AliasAs("input")] string input,
        [AliasAs("format")] string format = "json",
        [AliasAs("accessId")] string accessId = "YOUR_API_KEY"
    );
}