using Azure.DigitalTwins.Resolver.Fetchers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver.Tests
{
    public class FetchIntegrationTests
    {
        readonly Uri _remoteUri = new Uri(TestHelpers.GetTestRemoteModelRepo());
        readonly Uri _localUri = new Uri($"file://{TestHelpers.GetTestLocalModelRepo()}");
        Mock<ILogger> _logger;
        IModelFetcher _localFetcher;
        IModelFetcher _remoteFetcher;

        [SetUp]
        public void Setup()
        {
            _localFetcher = new LocalModelFetcher();
            _remoteFetcher = new RemoteModelFetcher();
            _logger = new Mock<ILogger>();
        }

        [Test]
        public async Task FetchLocalRepo()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            string expectedPath = DtmiConventions.ToPath(targetDtmi, _localUri.AbsolutePath);
            string fetcherPath = _localFetcher.GetPath(targetDtmi, _localUri);
            Assert.AreEqual(fetcherPath, expectedPath);

            // Resolution correctness is covered in ResolverIntegrationTests
            string content = await _localFetcher.FetchAsync(targetDtmi, _localUri, _logger.Object);
            Assert.IsNotNull(content);
 
            _logger.ValidateLog(StandardStrings.FetchingContent(fetcherPath), LogLevel.Information, Times.Once());
        }

        [Test]
        public void FetchLocalRepoDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            Uri invalidFileUri = new Uri("file://totally/fake/path/please");
            Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await _localFetcher.FetchAsync(targetDtmi, invalidFileUri, _logger.Object));

            _logger.ValidateLog(StandardStrings.ErrorAccessLocalRepository(invalidFileUri.AbsolutePath), LogLevel.Error, Times.Once());
        }

        [Test]
        public void FetchLocalRepoModelDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermojax;1";
            Assert.ThrowsAsync<FileNotFoundException>(async () => await _localFetcher.FetchAsync(targetDtmi, _localUri, _logger.Object));

            string expectedModelPath = DtmiConventions.ToPath(targetDtmi, _localUri.AbsolutePath);
            _logger.ValidateLog(StandardStrings.ErrorAccessLocalRepositoryModel(expectedModelPath), LogLevel.Error, Times.Once());
        }

        [Test]
        public async Task FetchRemoteRepo()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            string expectedPath = DtmiConventions.ToPath(targetDtmi, _remoteUri.AbsoluteUri);
            string fetcherPath = _remoteFetcher.GetPath(targetDtmi, _remoteUri);
            Assert.AreEqual(fetcherPath, expectedPath);

            // Resolution correctness is covered in ResolverIntegrationTests
            string content = await _remoteFetcher.FetchAsync(targetDtmi, _remoteUri, _logger.Object);
            Assert.IsNotNull(content);

            _logger.ValidateLog($"{StandardStrings.FetchingContent(fetcherPath)}", LogLevel.Information, Times.Once());
        }

        [Test]
        public void FetchRemoteRepoDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";
            Uri invalidRemoteUri = new Uri("http://localhost/fakeRepo/");
            HttpRequestException re = Assert.ThrowsAsync<HttpRequestException>(async () => await _remoteFetcher.FetchAsync(targetDtmi, invalidRemoteUri, _logger.Object));
            re.Message.Contains("404");

            // Don't need to verify a log entry as any http errors from fetcher are currently encapsulated in HttpRequestException
        }

        [Test]
        public void FetchRemoteRepoModelDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermojax;1";
            HttpRequestException re = Assert.ThrowsAsync<HttpRequestException>(async () => await _remoteFetcher.FetchAsync(targetDtmi, _remoteUri, _logger.Object));
            re.Message.Contains("404");

            // Don't need to verify a log entry as any http errors from fetcher are currently encapsulated in HttpRequestException
        }
    }
}