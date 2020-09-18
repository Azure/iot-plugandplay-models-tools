using NUnit.Framework;
using System.Runtime.InteropServices;

namespace Azure.DigitalTwins.Resolver.Tests
{
    public class DtmiConversionTests
    {
        [Test]
        public void DtmiToLocalPath()
        {
            string dtmiVariation1 = "dtmi:com:Example:Model;1";
            string dtmiVariation2 = "dtmi:com:example:Model;1";

            string expectedPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"dtmi\com\example\model-1.json" : "dtmi/com/example/model-1.json";

            Assert.AreEqual(expectedPath, Utility.DtmiToFilePath(dtmiVariation1));
            Assert.AreEqual(expectedPath, Utility.DtmiToFilePath(dtmiVariation2));

            string basePath1 = @"C:\fakeRegistry\";

            Assert.AreEqual($@"{basePath1}{expectedPath}", Utility.DtmiToFilePath(dtmiVariation1, basePath1));
        }

        [Test]
        public void DtmiToRemotePath()
        {
            string dtmiVariation1 = "dtmi:com:Example:Model;1";
            string dtmiVariation2 = "dtmi:com:example:Model;1";

            string registryBaseEndpoint = "http://localhost/registry";
            string expectedPath = $@"{registryBaseEndpoint}/dtmi/com/example/model-1.json";

            Assert.AreEqual(expectedPath, Utility.DtmiToRemotePath(dtmiVariation1, registryBaseEndpoint));
            Assert.AreEqual(expectedPath, Utility.DtmiToRemotePath(dtmiVariation2, registryBaseEndpoint));
        }
    }
}
