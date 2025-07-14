using HomeApi.Models.Response;
using Refit;

namespace HomeApi.Integration.Client;

public interface INominatimClient
{
    [Get("/search")]
    Task<List<NomatimApiResponse>> SearchAsync(
        [AliasAs("q")] string query,
        [AliasAs("format")] string format = "json",
        [AliasAs("limit")] int limit = 1);
}