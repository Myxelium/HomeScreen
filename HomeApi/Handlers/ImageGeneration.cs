using System.Dynamic;
using System.Reflection;
using HomeApi.Models.Configuration;
using MediatR;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using RazorLight;

namespace HomeApi.Handlers;

public static class ImageGeneration
{
    public record Command : IRequest<Stream>;

    public class Handler(
        IWebHostEnvironment env,
        IMediator mediator,
        IOptions<ApiConfiguration> apiConfiguration)
        : IRequestHandler<Command, Stream>
    {
        private readonly ApiConfiguration _apiConfiguration = apiConfiguration.Value;

        public async Task<Stream> Handle(Command request, CancellationToken cancellationToken)
        {
            var weather = await mediator.Send(new Weather.Command(), cancellationToken);
            var departureBoard = await mediator.Send(new DepartureBoard.Command(), cancellationToken);
            
            var model = new Models.Image
            {
                Weather = weather,
                TimeTable = departureBoard
            };
            
            if(weather is null)
                throw new Exception("Weather data not found");
            
            var engine = new RazorLightEngineBuilder()
                .SetOperatingAssembly(Assembly.GetExecutingAssembly())
                .UseEmbeddedResourcesProject(typeof(ImageGeneration))
                .UseMemoryCachingProvider()
                .Build();
            
            var path = Path.Combine(env.WebRootPath, "index.cshtml");

            var template = await File.ReadAllTextAsync(path, cancellationToken);

            dynamic viewBag = new ExpandoObject();
            viewBag.IsHighContrast = _apiConfiguration.EspConfiguration.IsHighContrastMode;
            
            var result = await engine.CompileRenderStringAsync("templateKey", template, model, viewBag: viewBag);

            if (!string.IsNullOrEmpty(result)) 
                return await CreateImage(result);
            
            throw new Exception("Failed to generate HTML content for image.");
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
            
            await page.SetContentAsync(htmlContent, new NavigationOptions { WaitUntil = [WaitUntilNavigation.Networkidle0] });
            return await page.ScreenshotStreamAsync(new ScreenshotOptions { Type = ScreenshotType.Jpeg, Quality = 60 });
        }
    }
}