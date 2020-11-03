using Azure.IoT.DeviceModelsRepository.Resolver.Fetchers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver.Tests
{
    public class FetchIntegrationTests
    {
        readonly Uri _remoteUri = new Uri(TestHelpers.TestRemoteModelRepository);
        readonly Uri _localUri = new Uri($"file://{TestHelpers.TestLocalModelRepository}");
        Mock<ILogger> _logger;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger>();
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task FetchLocalRepository(bool fetchExpanded)
        {
            // Casing is irrelevant for fetchers as they grab content based on file-path
            // which will always be lowercase. Casing IS important for the resolve flow
            // and is covered by tests there.
            string targetDtmi = "dtmi:com:example:temperaturecontroller;1";

            ResolverClientOptions options = fetchExpanded ?
                new ResolverClientOptions(DependencyResolutionOption.TryFromExpanded) :
                new ResolverClientOptions();

            string expectedPath = DtmiConventions.DtmiToQualifiedPath(targetDtmi, _localUri.AbsolutePath, fetchExpanded);
            LocalModelFetcher localFetcher = new LocalModelFetcher(_logger.Object, options);
            string fetcherPath = localFetcher.GetPath(targetDtmi, _localUri, fetchExpanded);
            Assert.AreEqual(fetcherPath, expectedPath);

            // Resolution correctness is covered in ResolverIntegrationTests
            FetchResult fetchResult = await localFetcher.FetchAsync(targetDtmi, _localUri);
            Assert.True(!string.IsNullOrEmpty(fetchResult.Definition));
            Assert.True(!string.IsNullOrEmpty(fetchResult.Path));
            Assert.AreEqual(fetchResult.FromExpanded, fetchExpanded);

            _logger.ValidateLog(StandardStrings.FetchingContent(fetcherPath), LogLevel.Trace, Times.Once());
        }

        [Test]
        public void FetchLocalRepositoryDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            Uri invalidFileUri = new Uri("file://totally/fake/path/please");
            LocalModelFetcher localFetcher = new LocalModelFetcher(_logger.Object, new ResolverClientOptions());
            Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await localFetcher.FetchAsync(targetDtmi, invalidFileUri));

            _logger.ValidateLog(StandardStrings.ErrorAccessLocalRepository(invalidFileUri.AbsolutePath), LogLevel.Error, Times.Once());
        }

        [Test]
        public void FetchLocalRepositoryModelDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermojax;1";
            LocalModelFetcher localFetcher = new LocalModelFetcher(_logger.Object, new ResolverClientOptions());
            Assert.ThrowsAsync<FileNotFoundException>(async () => await localFetcher.FetchAsync(targetDtmi, _localUri));

            string expectedModelPath = DtmiConventions.DtmiToQualifiedPath(targetDtmi, _localUri.AbsolutePath);
            _logger.ValidateLog(StandardStrings.ErrorAccessLocalRepositoryModel(expectedModelPath), LogLevel.Warning, Times.Once());
        }

        [TestCase(false)]
        // [TestCase(true)] - TODO: Uncomment when consistent remote repo available.
        public async Task FetchRemoteRepository(bool fetchExpanded)
        {
            string targetDtmi = "dtmi:com:example:temperaturecontroller;1";

            RemoteModelFetcher remoteFetcher = new RemoteModelFetcher(_logger.Object, new ResolverClientOptions());
            string expectedPath = DtmiConventions.DtmiToQualifiedPath(targetDtmi, _remoteUri.AbsoluteUri, fetchExpanded);
            string fetcherPath = remoteFetcher.GetPath(targetDtmi, _remoteUri, fetchExpanded);
            Assert.AreEqual(fetcherPath, expectedPath);

            // Resolution correctness is covered in ResolverIntegrationTests
            FetchResult fetchResult = await remoteFetcher.FetchAsync(targetDtmi, _remoteUri);
            Assert.True(!string.IsNullOrEmpty(fetchResult.Definition));
            Assert.True(!string.IsNullOrEmpty(fetchResult.Path));
            Assert.AreEqual(fetchResult.FromExpanded, fetchExpanded);

            _logger.ValidateLog($"{StandardStrings.FetchingContent(fetcherPath)}", LogLevel.Trace, Times.Once());
        }

        [Test]
        public void FetchRemoteRepositoryDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";
            Uri invalidRemoteUri = new Uri("https://localhost:80/fakeRepo/");
            RemoteModelFetcher remoteFetcher = new RemoteModelFetcher(_logger.Object, new ResolverClientOptions());
            Assert.ThrowsAsync<AggregateException>(async () => await remoteFetcher.FetchAsync(targetDtmi, invalidRemoteUri));
        }

        [Test]
        public void FetchRemoteRepositoryModelDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermojax;1";
            RemoteModelFetcher remoteFetcher = new RemoteModelFetcher(_logger.Object, new ResolverClientOptions());
            Assert.ThrowsAsync<RequestFailedException>(async () => await remoteFetcher.FetchAsync(targetDtmi, _remoteUri));
        }
    }
}
