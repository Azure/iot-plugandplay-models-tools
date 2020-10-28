using Azure.IoT.DeviceModelsRepository.Resolver;
using System.IO;
using System.Text;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    internal class ModelImporter
    {
        public static void Import(string modelContent, DirectoryInfo repository)
        {
            string rootId = new ModelQuery(modelContent).GetId();
            string createPath = DtmiConventions.DtmiToQualifiedPath(rootId, repository.FullName);
            (new FileInfo(createPath)).Directory.Create();
            File.WriteAllText(createPath, modelContent, Encoding.UTF8);
        }
    }
}
