using Azure.IoT.DeviceModelsRepository.Resolver;
using Azure.IoT.DeviceModelsRepository.Resolver.Extensions;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    public class Parsing
    {
        private readonly ILogger _logger;
        private readonly string _repository;

        public Parsing(string repository, ILogger logger){
            _logger = logger;
            _repository = repository;
        }

        public async Task<bool> IsValidDtdlFileAsync(FileInfo modelFile, bool strict)
        {
            _logger.LogInformation($"Using repository: '{_repository}'");
            ModelParser parser = GetParser();

            await parser.ParseAsync(new string[] { File.ReadAllText(modelFile.FullName) });
            if (strict)
            {
                return await modelFile.Validate();
            }

            return true;
        }

        public ModelParser GetParser()
        {
            ResolverClient client = GetResolver();
            ModelParser parser = new ModelParser
            {
                DtmiResolver = client.ParserDtmiResolver
            };
            return parser;
        }

        public ResolverClient GetResolver()
        {
            string repository = _repository;
            if (Validations.IsRelativePath(repository))
            {
                repository = Path.GetFullPath(repository);
            }

            return new ResolverClient(
                repository,
                new ResolverClientOptions(DependencyResolutionOption.TryFromExpanded),
                _logger);
        }

        public string GetRootDtmiFromFile(FileInfo fileName)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(fileName.FullName));
            JsonElement idElement = jsonDocument.RootElement.GetProperty("@id");
            return idElement.GetString();
        }
    }
}
