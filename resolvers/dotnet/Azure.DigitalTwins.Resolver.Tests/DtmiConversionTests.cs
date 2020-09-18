using NUnit.Framework;
using System.IO;

namespace Azure.DigitalTwins.Resolver.Tests
{
    public class DtmiConversionTests
    {
        [Test]
        public void DtmiToLocalPath()
        {
            string dtmiVariation1 = "dtmi:com:Example:Model;1";
            string dtmiVariation2 = "dtmi:com:example:Model;1";

            string registryBasePathWindows = @"C:\fakeRegistry\";
            string expectedPathWindows = $@"{registryBasePathWindows}/dtmi/com/example/model-1.json";

            Assert.AreEqual(expectedPathWindows, DtmiConventions.ToPath(dtmiVariation1, registryBasePathWindows));
            Assert.AreEqual(expectedPathWindows, DtmiConventions.ToPath(dtmiVariation2, registryBasePathWindows));

            string registryBasePathLinux = "/me/fakeRegistry";
            string expectedPathLinux = $@"{registryBasePathLinux}/dtmi/com/example/model-1.json";

            Assert.AreEqual(expectedPathLinux, DtmiConventions.ToPath(dtmiVariation1, registryBasePathLinux));
            Assert.AreEqual(expectedPathLinux, DtmiConventions.ToPath(dtmiVariation2, registryBasePathLinux));
        }

        [Test]
        public void DtmiToRemotePath()
        {
            string dtmiVariation1 = "dtmi:com:Example:Model;1";
            string dtmiVariation2 = "dtmi:com:example:Model;1";

            string registryBaseEndpoint = "http://localhost/registry";
            string expectedPath = $@"{registryBaseEndpoint}/dtmi/com/example/model-1.json";

            Assert.AreEqual(expectedPath, DtmiConventions.ToPath(dtmiVariation1, registryBaseEndpoint));
            Assert.AreEqual(expectedPath, DtmiConventions.ToPath(dtmiVariation2, registryBaseEndpoint));
        }
    }
}
