using HomeApi.Models.Response;
using Refit;

namespace HomeApi.Integration.Client;

public interface IAuroraClient
{
    [Get("/v1/")]
    Task<AuroraForecastApiResponse> GetForecastAsync(
        [AliasAs("type")] string type = "all",
        [AliasAs("lat")] string latitude = "0",
        [AliasAs("long")] string longitude = "0",
        [AliasAs("forecast")] string forecast = "false",
        [AliasAs("threeday")] string threeDay = "false");
}