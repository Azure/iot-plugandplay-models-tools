using System.IO;

namespace Azure.Iot.ModelsRepository.CLI.Tests
{
    public class TestHelpers
    {
        public enum ClientType
        {
            Local,
            Remote
        }

        public static string TestLocalModelRepository => 
            Path.GetFullPath(@"../../../../Azure.Iot.ModelsRepository.Tests/TestModelRepo");
    }
}
