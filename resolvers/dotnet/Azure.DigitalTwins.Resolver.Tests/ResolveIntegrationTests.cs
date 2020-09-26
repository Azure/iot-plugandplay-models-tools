using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver.Tests
{
    public class ResolveIntegrationTests
    {
        // TODO: Needs consistent remote registry
        // ResolverClient _remoteClient;
        ResolverClient _localClient;

        [SetUp]
        public void Setup()
        {
            _localClient = ResolverClient.FromLocalRegistry(TestHelpers.GetTestLocalModelRegistry());

            // TODO: Needs consistent remote registry
            // _remoteClient = ResolverClient.FromRemoteRegistry(TestHelpers.GetTestRemoteModelRegistry());
        }

        [TestCase("dtmi:com:example:Thermostat;1")]
        public async Task ResolveSingleModelNoDeps(string dtmi)
        {
            var result = await _localClient.ResolveAsync(dtmi);
            Assert.True(result.Keys.Count == 1);
            Assert.True(result.ContainsKey(dtmi));
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmi]) == dtmi);
        }

        [TestCase("dtmi:com:example:thermostat;1")]
        public void ResolveWithWrongCasingThrowsException(string dtmi)
        {
            Assert.ThrowsAsync<ResolverException>(async () => await _localClient.ResolveAsync(dtmi));
        }

        [TestCase("dtmi:com:example:thermojax;999")]
        public void ResolveInvalidDtmiThrowsException(string dtmi)
        {
            Assert.ThrowsAsync<ResolverException>(async () => await _localClient.ResolveAsync(dtmi));
        }

        [TestCase("dtmi:com:example:invalidmodel;1")]
        public void ResolveInvalidDtmiDepsThrowsException(string dtmi)
        {
            Assert.ThrowsAsync<ResolverException>(async () => await _localClient.ResolveAsync(dtmi));
        }

        [TestCase("dtmi:com:example:Thermostat;1", "dtmi:azure:deviceManagement:DeviceInformation;1")]
        public async Task ResolveMultipleModelsNoDeps(string dtmi1, string dtmi2)
        {
            var result = await _localClient.ResolveAsync(new string[] { dtmi1, dtmi2 });
            Assert.True(result.Keys.Count == 2);
            Assert.True(result.ContainsKey(dtmi1));
            Assert.True(result.ContainsKey(dtmi2));
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmi1]) == dtmi1);
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmi2]) == dtmi2);
        }

        [TestCase("dtmi:com:example:TemperatureController;1", 
                  "dtmi:com:example:Thermostat;1,dtmi:azure:deviceManagement:DeviceInformation;1")]
        public async Task ResolveSingleModelWithDepsAndLogger(string dtmi, string expectedDeps)
        {
            Mock<ILogger> _logger = new Mock<ILogger>();
            ResolverClient localClient = ResolverClient.FromLocalRegistry(TestHelpers.GetTestLocalModelRegistry(), _logger.Object);
           
            var result = await localClient.ResolveAsync(dtmi);
            var expectedDtmis = $"{dtmi},{expectedDeps}".Split(',', StringSplitOptions.RemoveEmptyEntries);

            Assert.True(result.Keys.Count == expectedDtmis.Length);
            foreach(var id in expectedDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }

            // Verifying log entries for a Process(...) run

            _logger.ValidateLog($"Client initialized with file content fetcher.", LogLevel.Information, Times.Once());
            
            _logger.ValidateLog($"Processing DTMI 'dtmi:com:example:TemperatureController;1'", LogLevel.Information, Times.Once());
            _logger.ValidateLog($"Attempting to retrieve model content from " +
                $"{DtmiConventions.ToPath(expectedDtmis[0], localClient.RegistryUri.AbsolutePath)}", LogLevel.Information, Times.Once());

            _logger.ValidateLog($"Discovered dependencies dtmi:com:example:Thermostat;1, dtmi:azure:deviceManagement:DeviceInformation;1", LogLevel.Information, Times.Once());
            
            _logger.ValidateLog($"Processing DTMI 'dtmi:com:example:Thermostat;1'", LogLevel.Information, Times.Once());
            _logger.ValidateLog($"Attempting to retrieve model content from " +
                $"{DtmiConventions.ToPath(expectedDtmis[1], localClient.RegistryUri.AbsolutePath)}", LogLevel.Information, Times.Once());

            _logger.ValidateLog($"Processing DTMI 'dtmi:azure:deviceManagement:DeviceInformation;1'", LogLevel.Information, Times.Once());
            _logger.ValidateLog($"Attempting to retrieve model content from " +
                $"{DtmiConventions.ToPath(expectedDtmis[2], localClient.RegistryUri.AbsolutePath)}", LogLevel.Information, Times.Once());
        }

        [TestCase("dtmi:com:example:Phone;2",
          "dtmi:com:example:TemperatureController;1",
          "dtmi:com:example:Thermostat;1," +
          "dtmi:azure:deviceManagement:DeviceInformation;1," +
          "dtmi:azure:deviceManagement:DeviceInformation;2," +
          "dtmi:com:example:Camera;3")]
        public async Task ResolveMultipleModelsWithDeps(string dtmi1, string dtmi2, string expectedDeps)
        {
            var result = await _localClient.ResolveAsync(dtmi1, dtmi2);
            var expectedDtmis = $"{dtmi1},{dtmi2},{expectedDeps}".Split(',', StringSplitOptions.RemoveEmptyEntries);

            Assert.True(result.Keys.Count == expectedDtmis.Length);
            foreach (var id in expectedDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }
        }

        [TestCase("dtmi:com:example:TemperatureController;1",
                  "dtmi:com:example:ConferenceRoom;1", // Model uses extends
                  "dtmi:com:example:Thermostat;1,dtmi:azure:deviceManagement:DeviceInformation;1,dtmi:com:example:Room;1")]
        public async Task ResolveMultipleModelsWithDepsExpand(string dtmi1, string dtmi2, string expectedDeps)
        {
            var result = await _localClient.ResolveAsync(dtmi1, dtmi2); // Uses ResolveAsync(params string[])
            var expectedDtmis = $"{dtmi1},{dtmi2},{expectedDeps}".Split(',', StringSplitOptions.RemoveEmptyEntries);

            Assert.True(result.Keys.Count == expectedDtmis.Length);
            foreach (var id in expectedDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }
        }

        [TestCase("dtmi:com:example:TemperatureController;1",
                  "dtmi:com:example:ColdStorage;1", // Model uses extends[]
                  "dtmi:com:example:Thermostat;1," +
                  "dtmi:azure:deviceManagement:DeviceInformation;1," +
                  "dtmi:com:example:Room;1," +
                  "dtmi:com:example:Freezer;1")]
        public async Task ResolveMultipleModelsWithDepsExpandVariant(string dtmi1, string dtmi2, string expectedDeps)
        {
            var result = await _localClient.ResolveAsync(dtmi1, dtmi2); // Uses ResolveAsync(params string[])
            var expectedDtmis = $"{dtmi1},{dtmi2},{expectedDeps}".Split(',', StringSplitOptions.RemoveEmptyEntries);

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
            var result = await _localClient.ResolveAsync(dtmiDupe1, dtmiDupe2);
            Assert.True(result.Keys.Count == 1);
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmiDupe1]) == dtmiDupe1);
        }
    }
}