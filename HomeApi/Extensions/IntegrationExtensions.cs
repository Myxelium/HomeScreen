using HomeApi.Models.Configuration;
using Microsoft.Extensions.Options;

namespace HomeApi.Extensions;

public static class IntegrationExtensions
{
    public static void ConfigureBaseAddress(this IHttpClientBuilder builder,
        Func<ApiConfiguration, string> getBaseUrl)
    {
        builder.ConfigureHttpClient((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<ApiConfiguration>>().Value;
            client.BaseAddress = new Uri(getBaseUrl(config));
        });
    }
}