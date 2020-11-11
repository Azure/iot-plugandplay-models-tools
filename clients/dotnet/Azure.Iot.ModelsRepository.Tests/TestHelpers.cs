using NUnit.Framework;
using System;
using System.IO;
using System.Text.Json;

namespace Azure.Iot.ModelsRepository.Tests
{
    public class TestHelpers
    {
        readonly static string _fallbackTestRemoteRepo = "https://devicemodels.azure.com/";
        public enum ClientType
        {
            Local,
            Remote
        }

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

        public static ResolverClient GetTestClient(ClientType clientType, ResolverClientOptions clientOptions = null)
        {
            if (clientType == ClientType.Local)
                return new ResolverClient(TestLocalModelRepository, clientOptions);
            if (clientType == ClientType.Remote)
                return new ResolverClient(TestRemoteModelRepository, clientOptions);

            throw new ArgumentException();
        }

        public static string TestDirectoryPath => Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory)));

        public static string TestLocalModelRepository => Path.Combine(TestDirectoryPath, "TestModelRepo");

        public static string TestRemoteModelRepository => Environment.GetEnvironmentVariable("PNP_TEST_REMOTE_REPO") ?? _fallbackTestRemoteRepo;
    }
}
