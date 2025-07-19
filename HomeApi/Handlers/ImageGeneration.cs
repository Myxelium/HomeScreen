using System.Dynamic;
using System.Reflection;
using HomeApi.Models.Configuration;
using MediatR;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using RazorLight;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

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
            
            var path = Path.Combine(_env.WebRootPath, "index.cshtml");

            var template = await File.ReadAllTextAsync(path, cancellationToken);

            var result = await engine.CompileRenderStringAsync("templateKey", template, model, viewBag: new ExpandoObject());

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

            return await stream.ToBmpStream();
        }
    }

    private static async Task<Stream> ToBmpStream(this Stream stream)
    {
        var image = await Image.LoadAsync<Rgba32>(stream);
        // Resize or crop to 800x480 if necessary
        image.Mutate(x => x.Resize(800, 480));

        // Reduce to 3-color e-paper palette
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];

                    // Compute perceived brightness (gray)
                    var brightness = 0.299f * pixel.R + 0.587f * pixel.G + 0.114f * pixel.B;

                    if (pixel.R > 150 && pixel.G < 80 && pixel.B < 80) // RED threshold
                    {
                        row[x] = new Rgba32(255, 0, 0); // Red
                    }
                    else if (brightness > 180)
                    {
                        row[x] = new Rgba32(255, 255, 255); // White
                    }
                    else
                    {
                        row[x] = new Rgba32(0, 0, 0); // Black
                    }
                }
            }
        });

        var bmpStream = new MemoryStream();
        var bmpEncoder = new BmpEncoder
        {
            BitsPerPixel = BmpBitsPerPixel.Pixel24
        };

        await image.SaveAsync(bmpStream, bmpEncoder);
        bmpStream.Position = 0;

        return bmpStream;
    }
}