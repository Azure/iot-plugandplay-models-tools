using Azure.IoT.DeviceModelsRepository.Resolver;
using System;
using System.IO;
using System.Text;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    internal class ModelImporter
    {
        public static void Import(string modelContent, DirectoryInfo repository)
        {
            string rootId = new Parsing(null).GetRootId(modelContent);
            string createPath = DtmiConventions.DtmiToQualifiedPath(rootId, repository.FullName);

            Outputs.WriteOutAsync($"- Importing model \"{rootId}\"...").Wait();
            if (File.Exists(createPath))
            {
                Outputs.WriteOutAsync(
                    $"Skipping \"{rootId}\". Model file already exists in repository.",
                    ConsoleColor.DarkCyan).Wait();
                return;
            }

            UTF8Encoding utf8WithoutBom = new UTF8Encoding(false);
            (new FileInfo(createPath)).Directory.Create();
            File.WriteAllText(createPath, modelContent, utf8WithoutBom);
        }
    }
}
