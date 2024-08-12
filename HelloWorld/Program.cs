using Serilog;
using System.Collections;

namespace HelloWorld
{
    internal class Program
    {
        protected Program()
        {

        }

        static async Task<int> Main()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessName()
                .WriteTo
                .Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Hello, World!");

            var delay = TimeSpan.FromSeconds(1);

            Log.Information("Waiting for {Delay}", delay);

            await Task.Delay(TimeSpan.FromSeconds(1));

            var exitCode = 0;

            Log.Information("Exiting with code {ExitCode}", exitCode);



            return exitCode;
        }
    }
}
