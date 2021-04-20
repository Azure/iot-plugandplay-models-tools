using Azure.IoT.ModelsRepository;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class ModelImporter
    {
        public static void Import(string modelContent, DirectoryInfo repository)
        {
            string rootId = ParsingUtils.GetRootId(modelContent);
            string createPath = DtmiConventions.GetModelUri(rootId, new Uri(repository.FullName)).AbsolutePath;

            Outputs.WriteOut($"- Importing model \"{rootId}\"...");
            if (File.Exists(createPath))
            {
                Outputs.WriteOut(
                    $"- Skipping \"{rootId}\". Model file already exists in repository.",
                    ConsoleColor.DarkCyan);
                return;
            }

            (new FileInfo(createPath)).Directory.Create();
            Outputs.WriteToFile(createPath, modelContent);
        }
    }
}
