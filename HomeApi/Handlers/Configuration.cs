using HomeApi.Models;
using HomeApi.Models.Configuration;
using MediatR;
using Microsoft.Extensions.Options;

namespace HomeApi.Handlers;

public static class Configuration
{
    public record Command : IRequest<MicroProcessorConfiguration>;

    public class Handler(IOptions<ApiConfiguration> configuration)
        : IRequestHandler<Command, MicroProcessorConfiguration>
    {
        private readonly ApiConfiguration _apiConfiguration = configuration.Value;

        public Task<MicroProcessorConfiguration> Handle(Command request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new MicroProcessorConfiguration 
            {
                InformationBoardImageUrl = _apiConfiguration.EspConfiguration.InformationBoardImageUrl,
                UpdateIntervalMinutes = _apiConfiguration.EspConfiguration.UpdateIntervalMinutes,
                BlackTextThreshold = _apiConfiguration.EspConfiguration.BlackTextThreshold,
                ContrastStrength = _apiConfiguration.EspConfiguration.ContrastStrength,
                DitheringStrength = _apiConfiguration.EspConfiguration.DitheringStrength,
                EnableDithering = _apiConfiguration.EspConfiguration.EnableDithering,
                EnhanceContrast = _apiConfiguration.EspConfiguration.EnhanceContrast
            });
        }
    }
}