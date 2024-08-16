using System.Diagnostics;
using System.Text.Json;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace HelloWorld
{
    internal class Program
    {
        private const int ExitFail = -1;
        private const int ExitSuccess = 0;

        protected Program()
        {

        }

        static async Task<int> Main()
        {
            var exitCode = ExitFail;
            var builder = Host.CreateApplicationBuilder();

            builder.Configuration.AddUserSecrets<Program>();

            var activitySource = new ActivitySource("Teqniqly.HelloWorld.K8S");

            var resourceAttributes = new Dictionary<string, object>
            {
                { "service.name", activitySource.Name },
                { "service.instance.id", activitySource.Name }
            };

            var resourceBuilder = ResourceBuilder.CreateDefault().AddAttributes(resourceAttributes);

            var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource(activitySource.Name)
                .AddHttpClientInstrumentation()
                .AddAzureMonitorTraceExporter(options =>
                {
                    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]!;
                })
                .Build();


            var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddHttpClientInstrumentation()
                .AddAzureMonitorMetricExporter(options =>
                {
                    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]!;
                })
                .Build();


            var loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.AddOpenTelemetry(o => o.AddAzureMonitorLogExporter(
                    options =>
                    {
                        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]!;
                    })
                    .SetResourceBuilder(resourceBuilder));

                loggingBuilder.AddConsole();
            });

            var logger = loggerFactory.CreateLogger<Program>();

            var apiBaseAddress = new Uri(builder.Configuration["ApiBaseAddress"]!);

            builder.Services
                .AddHttpClient("geoCode",
                    (client) => { client.BaseAddress = new Uri(apiBaseAddress, "geo/1.0/direct"); })
                .AddStandardResilienceHandler();

            builder.Services
                .AddHttpClient("weather",
                    (client) => { client.BaseAddress = new Uri(apiBaseAddress, "data/2.5/weather"); })
                .AddStandardResilienceHandler();

            var apiKey = builder.Configuration["ApiKey"]!;

            using (var activity = activitySource.StartActivity(ActivityKind.Server))
            {
                    try
                    {
                        using (var host = builder.Build())
                        {
                            {
                                var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();

                                var location = "Seattle,WA,USA";

                                logger.LogInformation("Getting geo code for {@Location}...", location);

                                var (lat, lon) = await GetGeoCode(location, httpClientFactory, apiKey);

                                logger.LogInformation("Geo code is {@GeoCode}", $"({lat},{lon})");

                                logger.LogInformation("Getting weather for {@Location}...", location);

                                var (temp, description) = await GetWeather((lat, lon), httpClientFactory, apiKey);

                                logger.LogInformation("Weather is {@Temp} {@Description}", temp, description);

                                exitCode = ExitSuccess;
                            }
                        }
                    }
                    catch (HttpRequestException httpRequestException)
                    {
                    
                        logger.LogError(
                            httpRequestException,
                            "An HTTP error occurred.");

                    }
                    catch (Exception exception)
                    {
                        logger.LogError(
                            exception,
                            "An application error occurred.");
                    }
                    finally
                    {
                        tracerProvider.Dispose();
                        meterProvider.Dispose();
                        loggerFactory.Dispose();
                    }
                
            }

            return exitCode;
        }

        private static async Task<(double temp, string description)> GetWeather(
            (double lat, double lon) geoCode,
            IHttpClientFactory httpClientFactory,
            string apiKey)
        {
            string weather;

            using (var weatherClient = httpClientFactory.CreateClient("weather"))
            {
                var weatherResponse = await weatherClient.GetAsync(
                        $"?lat={geoCode.lat}&lon={geoCode.lon}&units=imperial&appid={apiKey}");

                weatherResponse.EnsureSuccessStatusCode();
                weather = await weatherResponse.Content.ReadAsStringAsync();
            }

            double temp;
            string desc;

            using (var doc = JsonDocument.Parse(weather))
            {
                var root = doc.RootElement;

                temp = root
                    .GetProperty("main")
                    .GetProperty("temp")
                    .GetDouble();

                desc = root
                    .GetProperty("weather")[0]
                    .GetProperty("description")
                    .GetString()!;
            }

            return (temp, desc);
        }

        private static async Task<(double, double)> GetGeoCode(
            string location,
            IHttpClientFactory httpClientFactory,
            string apiKey)
        {
            string geoCode;

            using (var geoCodeClient = httpClientFactory.CreateClient("geoCode"))
            {
                var geoCodeResponse = await geoCodeClient.GetAsync(
                    $"?q='{location}'&limit=1&appid={apiKey}");

                geoCodeResponse.EnsureSuccessStatusCode();
                geoCode = await geoCodeResponse.Content.ReadAsStringAsync();
            }

            double lat;
            double lon;

            using (var doc = JsonDocument.Parse(geoCode))
            {
                var city = doc.RootElement[0];
                lat = city.GetProperty("lat").GetDouble();
                lon = city.GetProperty("lon").GetDouble();
            }

            return (lat, lon);
        }


    }
}
