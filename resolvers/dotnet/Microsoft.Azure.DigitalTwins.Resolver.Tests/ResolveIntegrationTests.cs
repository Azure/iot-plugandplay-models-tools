using NUnit.Framework;
using System.Threading.Tasks;

namespace Microsoft.Azure.DigitalTwins.Resolver.Tests
{
    public class ResolveIntegrationTests
    {
        // TODO: Needs consistent remote registry
        // ResolverClient remoteClient;
        ResolverClient localClient;

        [SetUp]
        public void Setup()
        {
            localClient = ResolverClient.FromLocalRegistry(TestHelpers.GetTestLocalModelRegistry());

            // TODO: Needs consistent remote registry
            // remoteClient = ResolverClient.FromRemoteRegistry(TestHelpers.GetTestRemoteModelRegistry());
        }

        [TestCase("dtmi:com:example:Thermostat;1")]
        public async Task ResolveSingleModelNoDeps(string dtmi)
        {
            var result = await localClient.ResolveAsync(dtmi);
            Assert.True(result.Keys.Count == 1);
            Assert.True(result.ContainsKey(dtmi));
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmi]) == dtmi);
        }

        [TestCase("dtmi:com:example:Thermostat;1", "dtmi:azure:deviceManagement:DeviceInformation;1")]
        public async Task ResolveMultipleModelsNoDeps(string dtmi1, string dtmi2)
        {
            var result = await localClient.ResolveAsync(new string[] { dtmi1, dtmi2 });
            Assert.True(result.Keys.Count == 2);
            Assert.True(result.ContainsKey(dtmi1));
            Assert.True(result.ContainsKey(dtmi2));
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmi1]) == dtmi1);
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmi2]) == dtmi2);
        }

        [TestCase("dtmi:com:example:TemperatureController;1", 
                  "dtmi:com:example:Thermostat;1,dtmi:azure:deviceManagement:DeviceInformation;1")]
        public async Task ResolveSingleModelWithDeps(string dtmi, string expectedDeps)
        {
            var result = await localClient.ResolveAsync(dtmi);
            var expectedDtmis = $@"{dtmi},{expectedDeps}".Split(',', System.StringSplitOptions.RemoveEmptyEntries);

            Assert.True(result.Keys.Count == expectedDtmis.Length);
            foreach(var id in expectedDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }
        }

        [TestCase("dtmi:com:example:TemperatureController;1",
                  "dtmi:com:example:ConferenceRoom;1", // Model uses extends
                  "dtmi:com:example:Thermostat;1,dtmi:azure:deviceManagement:DeviceInformation;1,dtmi:com:example:Room;1")]
        public async Task ResolveMultipleModelsWithDeps(string dtmi1, string dtmi2, string expectedDeps)
        {
            var result = await localClient.ResolveAsync(dtmi1, dtmi2); // Uses ResolveAsync(params string[])
            var expectedDtmis = $@"{dtmi1},{dtmi2},{expectedDeps}".Split(',', System.StringSplitOptions.RemoveEmptyEntries);

            Assert.True(result.Keys.Count == expectedDtmis.Length);
            foreach (var id in expectedDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }
        }

        [TestCase("dtmi:azure:deviceManagement:DeviceInformation;1", "dtmi:azure:deviceManagement:DeviceInformation;1")]
        public async Task ResolveEnsureNoDupes(string dtmiDupe1, string dtmiDupe2)
        {
            var result = await localClient.ResolveAsync(dtmiDupe1, dtmiDupe2);
            Assert.True(result.Keys.Count == 1);
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmiDupe1]) == dtmiDupe1);
        }

        [TestCase("dtmi:com:example:Phone;2",
                  "dtmi:com:example:TemperatureController;1",
                  "dtmi:com:example:Thermostat;1," +
                  "dtmi:azure:deviceManagement:DeviceInformation;1," +
                  "dtmi:azure:deviceManagement:DeviceInformation;2," +
                  "dtmi:com:example:Camera;3")]
        public async Task ResolveMultipleModelsWithDepsVariant(string dtmi1, string dtmi2, string expectedDeps)
        {
            var result = await localClient.ResolveAsync(dtmi1, dtmi2);
            var expectedDtmis = $@"{dtmi1},{dtmi2},{expectedDeps}".Split(',', System.StringSplitOptions.RemoveEmptyEntries);

            Assert.True(result.Keys.Count == expectedDtmis.Length);
            foreach (var id in expectedDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }
        }
    }
}