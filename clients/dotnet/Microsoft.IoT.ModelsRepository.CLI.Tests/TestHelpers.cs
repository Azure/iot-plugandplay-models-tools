using System.IO;
using System.Reflection;

namespace Microsoft.IoT.ModelsRepository.CLI.Tests
{
    public class TestHelpers
    {
        public enum ClientType
        {
            Local,
            Remote
        }

        public static string TestDirectoryPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string TestLocalModelRepository => Path.Combine(TestDirectoryPath, "TestModelRepo");
    }
}
