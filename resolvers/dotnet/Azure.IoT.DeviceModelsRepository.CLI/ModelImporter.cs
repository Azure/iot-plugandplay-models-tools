using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.IoT.DeviceModelsRepository.CLI.Exceptions;
using Azure.IoT.DeviceModelsRepository.Resolver;
using Microsoft.Extensions.Logging;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    internal static class ModelImporter
    {
        internal static async Task<IEnumerable<FileInfo>> ImportModels(FileInfo modelFile, DirectoryInfo repository, ILogger logger)
        {
            var fileText = await File.ReadAllTextAsync(modelFile.FullName);
            var model = JsonDocument.Parse(fileText);

            return ImportModels(model, modelFile.FullName, repository, logger);
        }
        private static IEnumerable<FileInfo> ImportModels(JsonDocument document, string fileName, DirectoryInfo repository, ILogger logger)
        {
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Array)
            {
                logger.LogTrace($"Array found in {fileName}");
                var enumerable = root.EnumerateArray();
                foreach (var modelItem in enumerable)
                {

                    yield return ImportModel(modelItem, fileName, repository, logger);
                }
            }
            else
            {
                logger.LogTrace($"Single item found in {fileName}");
                yield return ImportModel(root, fileName, repository, logger);

            }
        }
        private static FileInfo ImportModel(JsonElement modelItem, string fileName, DirectoryInfo repository, ILogger logger)
        {
            //Do DTMI verification
            var rootId = Validations.GetRootId(modelItem, fileName);
            if (!ResolverClient.IsValidDtmi(rootId.GetString()))
            {
                throw new InvalidDTMIException(rootId);
            }
            if (!Validations.ValidateDTMIs(modelItem, fileName, logger))
            {
                throw new InvalidDTMIException(fileName);
            }
            if (!Validations.ScanForReservedWords(modelItem.ToString(), logger))
            {
                throw new ValidationException($"File '{fileName}' contains reserved words. ");
            }

            // write file to repository location
            var newPath = DtmiConventions.DtmiToQualifiedPath(rootId.GetString(), repository.FullName);

            // TODO: consistent paths. Use global arg formatters.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                newPath = newPath.Replace("\\", "/");
            }

            if (!File.Exists(newPath))
            {
                CheckCreateDirectory(newPath);
                logger.LogTrace($"Writing new file to '{newPath}'. ");
                File.WriteAllText(newPath, modelItem.ToString(), Encoding.UTF8);
            }
            else
            {
                throw new IOException($"File '{newPath}' already exists. Please remove prior to execution.");
            }

            //return file info
            return new FileInfo(newPath);
        }

        private static void CheckCreateDirectory(string filePath)
        {
            var lastDirectoryIndex = filePath.LastIndexOf("/");
            var directory = filePath.Substring(0, lastDirectoryIndex);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
