using System.IO;
using System.Text.Json;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class IndexPageUtils
    {
        public static void WritePage(ModelIndex page)
        {
            string intendedFilePath = page.Links.Self;
            (new FileInfo(intendedFilePath)).Directory.Create();
            ConvertLinks(page);
            string indexJsonString = JsonSerializer.Serialize(page, ParsingUtils.DefaultJsonSerializerOptions);
            Outputs.WriteToFile(intendedFilePath, indexJsonString);
            Outputs.WriteOut($"Created index page: {intendedFilePath}");
        }

        private static void ConvertLinks(ModelIndex page)
        {
            if (!string.IsNullOrEmpty(page.Links.Self))
            {
                page.Links.Self = new FileInfo(page.Links.Self).Name;
            }

            if (!string.IsNullOrEmpty(page.Links.Next))
            {
                page.Links.Next = new FileInfo(page.Links.Next).Name;
            }

            if (!string.IsNullOrEmpty(page.Links.Prev))
            {
                page.Links.Prev = new FileInfo(page.Links.Prev).Name;
            }
        }
    }
}
