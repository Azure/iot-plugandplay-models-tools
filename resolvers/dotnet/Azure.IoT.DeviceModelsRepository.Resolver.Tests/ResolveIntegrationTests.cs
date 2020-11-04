using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver.Tests
{
    public class ResolveIntegrationTests
    {
        // TODO: Needs consistent remote repo
        // ResolverClient _remoteClient;
        ResolverClient _localClient;

        [SetUp]
        public void Setup()
        {
            _localClient = new ResolverClient(TestHelpers.TestLocalModelRepository);

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
                string.Format(StandardStrings.GenericResolverError, "dtmi:com:example:thermostat;1") +
                string.Format(StandardStrings.IncorrectDtmiCasing, "dtmi:com:example:thermostat;1", "dtmi:com:example:Thermostat;1");

            ResolverException re = Assert.ThrowsAsync<ResolverException>(async () => await _localClient.ResolveAsync(dtmi));
            Assert.AreEqual(re.Message, expectedExMsg);
        }

        [TestCase("dtmi:com:example:Thermostat:1")]
        [TestCase("dtmi:com:example::Thermostat;1")]
        [TestCase("com:example:Thermostat;1")]
        public void ResolveInvalidDtmiFormatThrowsException(string dtmi)
        {
            string expectedExMsg = $"{string.Format(StandardStrings.GenericResolverError, dtmi)}{string.Format(StandardStrings.InvalidDtmiFormat, dtmi)}";
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
            ResolverClient localClient = new ResolverClient(TestHelpers.TestLocalModelRepository, default);

            var result = await localClient.ResolveAsync(dtmi);
            var expectedDtmis = $"{dtmi},{expectedDeps}".Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            Assert.True(result.Keys.Count == expectedDtmis.Length);
            foreach (var id in expectedDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }

            // TODO: Evaluate using Azure.Core.TestFramework in future iteration.

            /*
             // Verifying log entries for a Process(...) run
            _logger.ValidateLog($"{StandardStrings.ClientInitWithFetcher(localClient.RepositoryUri.Scheme)}", LogLevel.Trace, Times.Once());

            _logger.ValidateLog($"{StandardStrings.ProcessingDtmi("dtmi:com:example:TemperatureController;1")}", LogLevel.Trace, Times.Once());
            _logger.ValidateLog($"{StandardStrings.FetchingContent(DtmiConventions.DtmiToQualifiedPath(expectedDtmis[0], localClient.RepositoryUri.AbsolutePath))}", LogLevel.Trace, Times.Once());

            _logger.ValidateLog($"{StandardStrings.DiscoveredDependencies(new List<string>() { "dtmi:com:example:Thermostat;1", "dtmi:azure:DeviceManagement:DeviceInformation;1" })}", LogLevel.Trace, Times.Once());

            _logger.ValidateLog($"{StandardStrings.ProcessingDtmi("dtmi:com:example:Thermostat;1")}", LogLevel.Trace, Times.Once());
            _logger.ValidateLog($"{StandardStrings.FetchingContent(DtmiConventions.DtmiToQualifiedPath(expectedDtmis[1], localClient.RepositoryUri.AbsolutePath))}", LogLevel.Trace, Times.Once());

            _logger.ValidateLog($"{StandardStrings.ProcessingDtmi("dtmi:azure:DeviceManagement:DeviceInformation;1")}", LogLevel.Trace, Times.Once());
            _logger.ValidateLog($"{StandardStrings.FetchingContent(DtmiConventions.DtmiToQualifiedPath(expectedDtmis[2], localClient.RepositoryUri.AbsolutePath))}", LogLevel.Trace, Times.Once());
            */
        }

        [TestCase("dtmi:com:example:Phone;2",
                  "dtmi:com:example:TemperatureController;1",
                  "dtmi:com:example:Thermostat;1," +
                  "dtmi:azure:DeviceManagement:DeviceInformation;1," +
                  "dtmi:azure:DeviceManagement:DeviceInformation;2," +
                  "dtmi:com:example:Camera;3")]
        public async Task ResolveMultipleModelsWithDeps(string dtmi1, string dtmi2, string expectedDeps)
        {
            var result = await _localClient.ResolveAsync(new[] { dtmi1, dtmi2 });
            var expectedDtmis = $"{dtmi1},{dtmi2},{expectedDeps}".Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

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
        public async Task ResolveMultipleModelsWithDepsFromExtends(string dtmi1, string dtmi2, string expectedDeps)
        {
            var result = await _localClient.ResolveAsync(new[] { dtmi1, dtmi2 });
            var expectedDtmis = $"{dtmi1},{dtmi2},{expectedDeps}".Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

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
        public async Task ResolveMultipleModelsWithDepsFromExtendsVariant(string dtmi1, string dtmi2, string expectedDeps)
        {
            var result = await _localClient.ResolveAsync(new[] { dtmi1, dtmi2 });
            var expectedDtmis = $"{dtmi1},{dtmi2},{expectedDeps}".Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            Assert.True(result.Keys.Count == expectedDtmis.Length);
            foreach (var id in expectedDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }
        }

        [TestCase("dtmi:com:example:base;1")]
        public async Task ResolveModelWithDepsFromExtendsInline(string dtmi)
        {
            var result = await _localClient.ResolveAsync(dtmi);

            Assert.True(result.Keys.Count == 1);
            Assert.True(result.ContainsKey(dtmi));
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmi]) == dtmi);
        }

        [TestCase("dtmi:com:example:base;2",
                  "dtmi:com:example:Freezer;1," +
                  "dtmi:com:example:Thermostat;1")]
        public async Task ResolveModelWithDepsFromExtendsInlineVariant(string dtmi, string expected)
        {
            var result = await _localClient.ResolveAsync(dtmi);
            var expectedDtmis = $"{dtmi},{expected}".Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

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
            var result = await _localClient.ResolveAsync(new[] { dtmiDupe1, dtmiDupe2 });
            Assert.True(result.Keys.Count == 1);
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmiDupe1]) == dtmiDupe1);
        }

        [TestCase("dtmi:com:example:TemperatureController;1")]
        public async Task ResolveSingleModelWithDepsDisableDependencyResolution(string dtmi)
        {
            ResolverClientOptions options = new ResolverClientOptions(DependencyResolutionOption.Disabled);
            ResolverClient localClient = new ResolverClient(
                TestHelpers.TestLocalModelRepository, options: options);

            var result = await localClient.ResolveAsync(dtmi);

            Assert.True(result.Keys.Count == 1);
            Assert.True(result.ContainsKey(dtmi));
            Assert.True(TestHelpers.ParseRootDtmiFromJson(result[dtmi]) == dtmi);
        }

        [TestCase(
            "dtmi:com:example:TemperatureController;1", // Expanded available locally.
            "dtmi:com:example:Thermostat;1,dtmi:azure:DeviceManagement:DeviceInformation;1",
            "local")]
        [TestCase(
            "dtmi:com:example:TemperatureController;1", // Expanded available remotely.
            "dtmi:com:example:Thermostat;1,dtmi:azure:DeviceManagement:DeviceInformation;1",
            "remote")]
        public async Task ResolveUseExpanded(string dtmi, string expectedDeps, string repoType)
        {
            var expectedDtmis = $"{dtmi},{expectedDeps}".Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            ResolverClientOptions options = new ResolverClientOptions(DependencyResolutionOption.TryFromExpanded);

            ResolverClient client = null;
            if (repoType == "local")
                client = new ResolverClient(TestHelpers.TestLocalModelRepository, options);

            if (repoType == "remote")
                client = new ResolverClient(TestHelpers.TestRemoteModelRepository, options);

            var result = await client.ResolveAsync(dtmi);

            Assert.True(result.Keys.Count == expectedDtmis.Length);
            foreach (var id in expectedDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }

            // TODO: Evaluate using Azure.Core.TestFramework in future iteration.

            /*
            string expectedPath = DtmiConventions.DtmiToQualifiedPath(
                dtmi,
                repoType == "local" ? client.RepositoryUri.AbsolutePath : client.RepositoryUri.AbsoluteUri,
                fromExpanded: true);
            _logger.ValidateLog(StandardStrings.FetchingContent(expectedPath), LogLevel.Trace, Times.Once());
            */
        }

        [TestCase("dtmi:com:example:TemperatureController;1," +  // Expanded available.
                  "dtmi:com:example:Thermostat;1," +
                  "dtmi:azure:DeviceManagement:DeviceInformation;1",
                  "dtmi:com:example:ColdStorage;1," + // Model uses extends[], No Expanded available.
                  "dtmi:com:example:Room;1," +
                  "dtmi:com:example:Freezer;1")]
        public async Task ResolveUseExpandedPartialMultipleDtmi(string dtmisExpanded, string dtmisNonExpanded)
        {
            string[] expandedDtmis = dtmisExpanded.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string[] nonExpandedDtmis = dtmisNonExpanded.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string[] totalDtmis = expandedDtmis.Concat(nonExpandedDtmis).ToArray();

            ResolverClientOptions options = new ResolverClientOptions(DependencyResolutionOption.TryFromExpanded);
            ResolverClient localClient = new ResolverClient(TestHelpers.TestLocalModelRepository, options: options);

            // Multi-resolve dtmi:com:example:TemperatureController;1 + dtmi:com:example:ColdStorage;1
            var result = await localClient.ResolveAsync(new[] { expandedDtmis[0], nonExpandedDtmis[0] });

            Assert.True(result.Keys.Count == totalDtmis.Length);
            foreach (string id in totalDtmis)
            {
                Assert.True(result.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(result[id]) == id);
            }

            // TODO: Evaluate using Azure.Core.TestFramework in future iteration.

            /*
            string expandedModelPath = DtmiConventions.DtmiToQualifiedPath(expandedDtmis[0], localClient.RepositoryUri.AbsolutePath, fromExpanded: true);
            _logger.ValidateLog(StandardStrings.FetchingContent(expandedModelPath), LogLevel.Trace, Times.Once());

            foreach (string dtmi in nonExpandedDtmis)
            {
                string expectedPath = DtmiConventions.DtmiToQualifiedPath(dtmi, localClient.RepositoryUri.AbsolutePath, fromExpanded: true);
                _logger.ValidateLog(StandardStrings.FetchingContent(expectedPath), LogLevel.Trace, Times.Once());
                _logger.ValidateLog(StandardStrings.ErrorAccessLocalRepositoryModel(expectedPath), LogLevel.Warning, Times.Once());
            }
            */
        }
    }
}
