using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;

namespace dtdl2_validator
{
    class Program
    {
        static void Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource(5000);

            var cliArgs = new Dictionary<string, string>()
            {
                {"-f", "file" },
                {"--file", "file" },
                {"-r", "resolver" },
                {"--resolver", "resolver" },
                {"-bf", "baseFolder" },
                {"--baseFolder", "baseFolder" }
            };

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args, cliArgs)
                .Build();
            
            ILogger logger = LoggerFactory.Create(builder =>
                builder
                .AddConfiguration(config.GetSection("Logging"))
                .AddDebug()
                .AddConsole()
            ).CreateLogger<ModelValidationService>();

            var validator = new ModelValidationService(config, logger);
            validator.ExecuteAsync(cancellationTokenSource.Token).Wait();
        }
    }
}
