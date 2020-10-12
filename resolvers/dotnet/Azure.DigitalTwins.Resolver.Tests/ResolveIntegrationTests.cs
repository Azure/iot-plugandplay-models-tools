using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Azure.DigitalTwins.Resolver.RepositoryHandler;

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

        [TestCase(
            "dtmi:com:example:TemperatureController;1", // Expanded available locally.
            "dtmi:com:example:Thermostat;1,dtmi:azure:DeviceManagement:DeviceInformation;1",
            RepositoryTypeCategory.LocalUri)]
        [TestCase(
            "dtmi:com:example:TemperatureController;1", // Expanded available remotely.
            "dtmi:com:example:Thermostat;1,dtmi:azure:DeviceManagement:DeviceInformation;1",
            RepositoryTypeCategory.RemoteUri)]
        public async Task ResolveUseExpanded(string dtmi, string expectedDeps, RepositoryTypeCategory clientType)
        {
            Mock<ILogger> _logger = new Mock<ILogger>();
            var expectedDtmis = $"{dtmi},{expectedDeps}".Split(',', StringSplitOptions.RemoveEmptyEntries);

            ResolutionSettings settings = new ResolutionSettings(
                calculateDependencies: true,
                usePreComputedDependencies: true);

            ResolverClient client = null;
            if (clientType == RepositoryTypeCategory.LocalUri)
                client = ResolverClient.FromLocalRepository(
                    TestHelpers.GetTestLocalModelRepository(),
                    settings: settings,
                    logger: _logger.Object);

            if (clientType == RepositoryTypeCategory.RemoteUri)
                client = ResolverClient.FromRemoteRepository(
                    TestHelpers.GetTestRemoteModelRepository(),
                    settings: settings,
                    logger: _logger.Object);

            var result = await client.ResolveAsync(dtmi);

            Assert.True(result.Keys.Count == expectedDtmis.Length);
            foreach (var id in expectedDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }

            string expectedPath = DtmiConventions.ToPath(
                dtmi, 
                clientType == RepositoryTypeCategory.LocalUri ? client.RepositoryUri.AbsolutePath : client.RepositoryUri.AbsoluteUri,
                expanded: true);
            _logger.ValidateLog(StandardStrings.FetchingContent(expectedPath), LogLevel.Information, Times.Once());
        }

        [TestCase("dtmi:com:example:TemperatureController;1," +  // Expanded available.
                  "dtmi:com:example:Thermostat;1," +
                  "dtmi:azure:DeviceManagement:DeviceInformation;1",
                  "dtmi:com:example:ColdStorage;1," + // Model uses extends[], No Expanded available.
                  "dtmi:com:example:Room;1," +
                  "dtmi:com:example:Freezer;1")]
        public async Task ResolveUseExpandedPartialMultipleDtmi(string dtmisExpanded, string dtmisNonExpanded)
        {
            Mock<ILogger> _logger = new Mock<ILogger>();
            string[] expandedDtmis = dtmisExpanded.Split(',', StringSplitOptions.RemoveEmptyEntries);
            string[] nonExpandedDtmis = dtmisNonExpanded.Split(',', StringSplitOptions.RemoveEmptyEntries);
            string[] totalDtmis = expandedDtmis.Concat(nonExpandedDtmis).ToArray();

            ResolutionSettings settings = new ResolutionSettings(
                calculateDependencies: true,
                usePreComputedDependencies: true);

            ResolverClient localClient = ResolverClient.FromLocalRepository(
                TestHelpers.GetTestLocalModelRepository(),
                settings: settings,
                logger: _logger.Object);

            // Multi-resolve dtmi:com:example:TemperatureController;1 + dtmi:com:example:ColdStorage;1
            var result = await localClient.ResolveAsync(expandedDtmis[0], nonExpandedDtmis[0]);

            Assert.True(result.Keys.Count == totalDtmis.Length);
            foreach (string id in totalDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }

            string expandedModelPath = DtmiConventions.ToPath(expandedDtmis[0], localClient.RepositoryUri.AbsolutePath, expanded: true);
            _logger.ValidateLog(StandardStrings.FetchingContent(expandedModelPath), LogLevel.Information, Times.Once());

            foreach (string dtmi in nonExpandedDtmis)
            {
                string expectedPath = DtmiConventions.ToPath(dtmi, localClient.RepositoryUri.AbsolutePath, expanded: true);
                _logger.ValidateLog(StandardStrings.FetchingContent(expectedPath), LogLevel.Information, Times.Once());
                _logger.ValidateLog(StandardStrings.ErrorAccessLocalRepositoryModel(expectedPath), LogLevel.Warning, Times.Once());
            }
        }
    }
}
