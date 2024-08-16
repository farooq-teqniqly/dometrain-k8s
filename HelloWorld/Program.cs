using Serilog;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Templates.Themes;
using SerilogTracing.Expressions;
using SerilogTracing;
using Serilog.Events;

namespace HelloWorld
{
    internal class Program
    {
        protected Program()
        {

        }

        static async Task Main()
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Configuration.AddUserSecrets<Program>();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(Formatters.CreateConsoleTextFormatter(TemplateTheme.Code))
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);

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

            using (new ActivityListenerConfiguration().TraceToSharedLogger())
            {
                using (var activity = Log.Logger.StartActivity("Get weather flow."))
                {
                    try
                    {
                        using (var host = builder.Build())
                        {
                            {
                                var httpClientFactory = host.Services.GetRequiredService<IHttpClientFactory>();

                                var location = "Seattle,WA,USA";

                                Log.Information("Getting geo code for {@Location}...", location);

                                var (lat, lon) = await GetGeoCode(location, httpClientFactory, apiKey);

                                Log.Information("Geo code is {@GeoCode}", $"({lat},{lon})");

                                Log.Information("Getting weather for {@Location}...", location);

                                var (temp, description) = await GetWeather((lat, lon), httpClientFactory, apiKey);

                                Log.Information("Weather is {@Temp} {@Description}", temp, description);

                                activity.Complete();
                            }
                        }
                    }
                    catch (HttpRequestException httpRequestException)
                    {
                        activity.Complete(LogEventLevel.Error, httpRequestException);

                        Log.Error(
                            httpRequestException,
                            "An HTTP error occurred.");

                        throw;

                    }
                    catch (Exception exception)
                    {
                        activity.Complete(LogEventLevel.Error, exception);

                        Log.Error(
                            exception,
                            "An application error occurred.");

                        throw;
                    }
                    finally
                    {
                        await Log.CloseAndFlushAsync();
                    }
                }
            }
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
