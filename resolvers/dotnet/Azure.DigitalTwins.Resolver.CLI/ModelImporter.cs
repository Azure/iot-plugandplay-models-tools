using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.DigitalTwins.Resolver.CLI.Exceptions;
using Microsoft.Extensions.Logging;

namespace Azure.DigitalTwins.Resolver.CLI
{
    internal static class ModelImporter
    {
        internal static async Task<IEnumerable<FileInfo>> ImportModels(FileInfo modelFile, DirectoryInfo repository, bool force, ILogger logger)
        {
            var fileText = await File.ReadAllTextAsync(modelFile.FullName);
            var model = JsonDocument.Parse(fileText);

            return importModels(model, modelFile.FullName, repository, force, logger);
        }
        private static IEnumerable<FileInfo> importModels(JsonDocument document, string fileName, DirectoryInfo repository, bool force, ILogger logger)
        {
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Array)
            {
                logger.LogTrace($"Array found in {fileName}");
                var enumerable = root.EnumerateArray();
                foreach (var modelItem in enumerable)
                {

                    yield return importModel(modelItem, fileName, repository, logger, force);
                }
            }
            else
            {
                logger.LogTrace($"Single item found in {fileName}");
                yield return importModel(root, fileName, repository, logger, force);

            }
        }
        private static FileInfo importModel(JsonElement modelItem, string fileName, DirectoryInfo repository, ILogger logger, bool force)
        {
            //Do DTMI verification
            var rootId = Validations.GetRootId(modelItem, fileName);
            if (!RepositoryHandler.IsValidDtmi(rootId.GetString()))
            {
                throw new InvalidDTMIException(rootId);
            }
            if (!Validations.ValidateDTMIs(modelItem, fileName, logger))
            {
                throw new InvalidDTMIException(fileName);
            }
            if(!Validations.ScanForReservedWords(modelItem.ToString(), logger))
            {
                throw new ValidationException($"File '{fileName}' contains reserved words.");
            }

            // write file to repository location
            var newFile = DtmiConventions.ToPath(rootId.GetString());
            var newPath = Path.Join(repository.FullName, newFile);
            if (!File.Exists(newPath))
            {
                CheckCreateDirectory(newPath);
                logger.LogTrace($"Writing new file to '{newPath}'.");
                File.WriteAllText(newPath, modelItem.ToString(), Encoding.UTF8);
            } else if (force) {
                logger.LogWarning($"File '{newPath} already exists. Overwriting...");
                File.WriteAllText(newPath, modelItem.ToString(), Encoding.UTF8);
            } else {
                throw new IOException($"File '{newPath} already exists. Remove or use '--force' to overwrite.");
            }

            //return file info
            return new FileInfo(newPath);
        }

        private static void CheckCreateDirectory(string filePath)
        {
            var lastDirectoryIndex = filePath.LastIndexOf(Path.DirectorySeparatorChar);
            var directory = filePath.Substring(0, lastDirectoryIndex);
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}