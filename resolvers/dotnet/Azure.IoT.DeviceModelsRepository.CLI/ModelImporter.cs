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
            string rootId = Parsing.GetRootId(modelContent);
            string createPath = DtmiConventions.DtmiToQualifiedPath(rootId, repository.FullName);

            Outputs.WriteOut($"- Importing model \"{rootId}\"...");
            if (File.Exists(createPath))
            {
                Outputs.WriteOut(
                    $"Skipping \"{rootId}\". Model file already exists in repository.",
                    ConsoleColor.DarkCyan);
                return;
            }

            (new FileInfo(createPath)).Directory.Create();
            File.WriteAllText(createPath, modelContent, Encoding.UTF8);
        }
    }
}
