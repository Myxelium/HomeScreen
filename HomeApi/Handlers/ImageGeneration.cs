using System.Reflection;
using HomeApi.Models;
using HomeApi.Models.Configuration;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using RazorLight;

namespace HomeApi.Handlers;

public static class ImageGeneration
{
    public record Command : IRequest<Stream>;

    public class Handler : IRequestHandler<Command, Stream>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IMediator _mediator;
        public Handler(
            IOptions<ApiConfiguration> apiConfiguration,
            ILogger<Handler> logger, IWebHostEnvironment env, IMediator mediator)
        {
            _logger = logger;
            _env = env;
            _mediator = mediator;
        }

        public async Task<Stream> Handle(Command request, CancellationToken cancellationToken)
        {
            var weather = await _mediator.Send(new Weather.Command(), cancellationToken);
            var departureBoard = await _mediator.Send(new DepartureBoard.Command(), cancellationToken);
            
            var model = new Image
            {
                Weather = weather,
                TimeTable = departureBoard
            };
            
            if(weather is null)
                throw new Exception("Weather data not found");
            
            var engine = new RazorLightEngineBuilder().SetOperatingAssembly(Assembly.GetExecutingAssembly())
                .UseEmbeddedResourcesProject(typeof(ImageGeneration)).UseMemoryCachingProvider().Build();
            var path = Path.Combine(_env.WebRootPath, "index.cshtml");
            
            var template = await File.ReadAllTextAsync(path);

            var result = await engine.CompileRenderStringAsync("templateKey", template, model);

            return await CreateImage(result);
        }

        private static async Task<Stream> CreateImage(string htmlContent)
        {
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = ["--disable-gpu"]
            });
            var page = await browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 800,
                Height = 480
            });
            await page.SetContentAsync(htmlContent, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });
            var stream = await page.ScreenshotStreamAsync(new ScreenshotOptions { Type = ScreenshotType.Png });
            return stream;
        }
    }
}