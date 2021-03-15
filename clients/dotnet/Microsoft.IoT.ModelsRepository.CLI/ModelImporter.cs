using Azure.Iot.ModelsRepository;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.IoT.ModelsRepository.CLI
{
    internal class ModelImporter
    {
        public async static Task ImportAsync(string modelContent, DirectoryInfo repository)
        {
            string rootId = new Parsing(null).GetRootId(modelContent);
            string createPath = DtmiConventions.GetModelUri(rootId, new Uri(repository.FullName)).AbsolutePath;

            Outputs.WriteOut($"- Importing model \"{rootId}\"...");
            if (File.Exists(createPath))
            {
                Outputs.WriteOut(
                    $"Skipping \"{rootId}\". Model file already exists in repository.",
                    ConsoleColor.DarkCyan);
                return;
            }

            UTF8Encoding utf8WithoutBom = new UTF8Encoding(false);
            (new FileInfo(createPath)).Directory.Create();
            await File.WriteAllTextAsync(createPath, modelContent, utf8WithoutBom);
        }
    }
}
