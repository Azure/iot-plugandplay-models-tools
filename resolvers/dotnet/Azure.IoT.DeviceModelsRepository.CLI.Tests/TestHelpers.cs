using System;
using System.IO;

namespace Azure.IoT.DeviceModelsRepository.CLI.Tests
{
    public class TestHelpers
    {
        public enum ClientType
        {
            Local,
            Remote
        }

        public static string TestLocalModelRepository => 
            Path.GetFullPath(@"../../../../Azure.IoT.DeviceModelsRepository.Resolver.Tests/TestModelRepo");
    }
}
