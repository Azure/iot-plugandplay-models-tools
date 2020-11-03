using NUnit.Framework;
using System;
using System.IO;
using System.Text.Json;

namespace Azure.IoT.DeviceModelsRepository.Resolver.Tests
{
    class TestHelpers
    {
        readonly static string _fallbackTestRemoteRepo = "https://devicemodeltest.azureedge.net/";

        public static string ParseRootDtmiFromJson(string json)
        {
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            };

            string dtmi = string.Empty;
            using (JsonDocument document = JsonDocument.Parse(json, options))
            {
                dtmi = document.RootElement.GetProperty("@id").GetString();
            }
            return dtmi;
        }

        public static string TestDirectoryPath => Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory)));

        public static string TestLocalModelRepository => Path.Combine(TestDirectoryPath, "TestModelRepo");

        public static string TestRemoteModelRepository => Environment.GetEnvironmentVariable("PNP_TEST_REMOTE_REPO") ?? _fallbackTestRemoteRepo;
    }
}
