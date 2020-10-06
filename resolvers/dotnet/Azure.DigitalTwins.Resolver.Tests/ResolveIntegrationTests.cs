using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver.Tests
{
    public class ResolveIntegrationTests
    {
        // TODO: Needs consistent remote repo
        // ResolverClient _remoteClient;
        ResolverClient _localClient;

        [SetUp]
        public void Setup()
        {
            _localClient = ResolverClient.FromLocalRepository(TestHelpers.GetTestLocalModelRepository());

            // TODO: Needs consistent remote repo
            // _remoteClient = ResolverClient.FromRemoteRepository(TestHelpers.GetTestRemoteModelRegistry());
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
            string expectedExMsg =
                $"{StandardStrings.GenericResolverError("dtmi:com:example:thermostat;1")}" +
                $"{StandardStrings.IncorrectDtmiCasing("dtmi:com:example:thermostat;1","dtmi:com:example:Thermostat;1")}";

            ResolverException re = Assert.ThrowsAsync<ResolverException>(async () => await _localClient.ResolveAsync(dtmi));
            Assert.AreEqual(re.Message, expectedExMsg);
        }

        [TestCase("dtmi:com:example:Thermostat:1")]
        [TestCase("dtmi:com:example::Thermostat;1")]
        [TestCase("com:example:Thermostat;1")]
        public void ResolveInvalidDtmiFormatThrowsException(string dtmi)
        {
            string expectedExMsg = $"{StandardStrings.GenericResolverError(dtmi)}{StandardStrings.InvalidDtmiFormat(dtmi)}";
            ResolverException re = Assert.ThrowsAsync<ResolverException>(async () => await _localClient.ResolveAsync(dtmi));
            Assert.AreEqual(re.Message, expectedExMsg);
        }

        [TestCase("dtmi:com:example:thermojax;999")]
        public void ResolveNoneExistantDtmiContentThrowsException(string dtmi)
        {
            Assert.ThrowsAsync<ResolverException>(async () => await _localClient.ResolveAsync(dtmi));
        }

        [TestCase("dtmi:com:example:invalidmodel;1")]
        public void ResolveInvalidDtmiDepsThrowsException(string dtmi)
        {
            Assert.ThrowsAsync<ResolverException>(async () => await _localClient.ResolveAsync(dtmi));
        }

        [TestCase("dtmi:com:example:Thermostat;1", "dtmi:azure:DeviceManagement:DeviceInformation;1")]
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
                  "dtmi:com:example:Thermostat;1,dtmi:azure:DeviceManagement:DeviceInformation;1")]
        public async Task ResolveSingleModelWithDepsAndLogger(string dtmi, string expectedDeps)
        {
            Mock<ILogger> _logger = new Mock<ILogger>();
            ResolverClient localClient = ResolverClient.FromLocalRepository(TestHelpers.GetTestLocalModelRepository(), _logger.Object);
           
            var result = await localClient.ResolveAsync(dtmi);
            var expectedDtmis = $"{dtmi},{expectedDeps}".Split(',', StringSplitOptions.RemoveEmptyEntries);

            Assert.True(result.Keys.Count == expectedDtmis.Length);
            foreach(var id in expectedDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }

            // Verifying log entries for a Process(...) run

            _logger.ValidateLog($"{StandardStrings.ClientInitWithFetcher(localClient.RepositoryUri.Scheme)}", LogLevel.Information, Times.Once());
            
            _logger.ValidateLog($"{StandardStrings.ProcessingDtmi("dtmi:com:example:TemperatureController;1")}", LogLevel.Information, Times.Once());
            _logger.ValidateLog($"{StandardStrings.FetchingContent(DtmiConventions.ToPath(expectedDtmis[0], localClient.RepositoryUri.AbsolutePath))}", LogLevel.Information, Times.Once());

            _logger.ValidateLog($"{StandardStrings.DiscoveredDependencies(new List<string>() { "dtmi:com:example:Thermostat;1", "dtmi:azure:DeviceManagement:DeviceInformation;1" })}", LogLevel.Information, Times.Once());

            _logger.ValidateLog($"{StandardStrings.ProcessingDtmi("dtmi:com:example:Thermostat;1")}", LogLevel.Information, Times.Once());
            _logger.ValidateLog($"{StandardStrings.FetchingContent(DtmiConventions.ToPath(expectedDtmis[1], localClient.RepositoryUri.AbsolutePath))}", LogLevel.Information, Times.Once());

            _logger.ValidateLog($"{StandardStrings.ProcessingDtmi("dtmi:azure:DeviceManagement:DeviceInformation;1")}", LogLevel.Information, Times.Once());
            _logger.ValidateLog($"{StandardStrings.FetchingContent(DtmiConventions.ToPath(expectedDtmis[2], localClient.RepositoryUri.AbsolutePath))}", LogLevel.Information, Times.Once());
        }

        [TestCase("dtmi:com:example:Phone;2",
                  "dtmi:com:example:TemperatureController;1",
                  "dtmi:com:example:Thermostat;1," +
                  "dtmi:azure:DeviceManagement:DeviceInformation;1," +
                  "dtmi:azure:DeviceManagement:DeviceInformation;2," +
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
                  "dtmi:com:example:Thermostat;1,dtmi:azure:DeviceManagement:DeviceInformation;1,dtmi:com:example:Room;1")]
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
                  "dtmi:azure:DeviceManagement:DeviceInformation;1," +
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

        [TestCase("dtmi:azure:DeviceManagement:DeviceInformation;1", "dtmi:azure:DeviceManagement:DeviceInformation;1")]
        public async Task ResolveEnsureNoDupes(string dtmiDupe1, string dtmiDupe2)
        {
            var result = await _localClient.ResolveAsync(dtmiDupe1, dtmiDupe2);
            Assert.True(result.Keys.Count == 1);
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmiDupe1]) == dtmiDupe1);
        }
    }
}