using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Iot.ModelsRepository.CLI
{
    internal class ModelImporter
    {
        public async static Task ImportAsync(string modelContent, DirectoryInfo repository)
        {
            string rootId = new Parsing(null).GetRootId(modelContent);
            string createPath = DtmiConventions.DtmiToQualifiedPath(rootId, repository.FullName);

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
