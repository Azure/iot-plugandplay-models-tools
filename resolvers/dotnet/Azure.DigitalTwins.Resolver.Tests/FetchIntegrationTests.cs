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
        readonly Uri _remoteUri = new Uri(TestHelpers.GetTestRemoteModelRegistry());
        readonly Uri _localUri = new Uri($"file://{TestHelpers.GetTestLocalModelRegistry()}");
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
        public async Task FetchLocalRegistry()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            // Resolution correctness is covered in ResolverIntegrationTests
            string content = await _localFetcher.FetchAsync(targetDtmi, _localUri, _logger.Object);
            Assert.IsNotNull(content);

            string expectedPath = DtmiConventions.ToPath(targetDtmi, _localUri.AbsolutePath);
            _logger.ValidateLog($"Attempting to retrieve model content from {expectedPath}", LogLevel.Information, Times.Once());
        }

        [Test]
        public void FetchLocalRegistryDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            Uri invalidFileUri = new Uri("file://totally/fake/path/please");
            Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await _localFetcher.FetchAsync(targetDtmi, invalidFileUri, _logger.Object));

            _logger.ValidateLog($"Local registry directory '{invalidFileUri.AbsolutePath}' not found or not accessible.", LogLevel.Error, Times.Once());
        }

        [Test]
        public void FetchLocalRegistryModelDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermojax;1";
            Assert.ThrowsAsync<FileNotFoundException>(async () => await _localFetcher.FetchAsync(targetDtmi, _localUri, _logger.Object));

            string expectedPath = DtmiConventions.ToPath(targetDtmi, _localUri.AbsolutePath);
            _logger.ValidateLog($"Model file '{expectedPath}' not found or not accessible in local registry directory '{_localUri.AbsolutePath}'", LogLevel.Error, Times.Once());
        }

        [Test]
        public async Task FetchRemoteRegistry()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            // Resolution correctness is covered in ResolverIntegrationTests
            string content = await _remoteFetcher.FetchAsync(targetDtmi, _remoteUri, _logger.Object);
            Assert.IsNotNull(content);

            string expectedPath = DtmiConventions.ToPath(targetDtmi, _remoteUri.AbsoluteUri);
            _logger.ValidateLog($"Attempting to retrieve model content from {expectedPath}", LogLevel.Information, Times.Once());
        }

        [Test]
        public void FetchRemoteRegistryDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";
            Uri invalidRemoteUri = new Uri("http://localhost/fakeRegistry/");
            Assert.ThrowsAsync<HttpRequestException>(async () => await _remoteFetcher.FetchAsync(targetDtmi, invalidRemoteUri, _logger.Object));

            // Don't need to verify a log entry as any http errors from fetcher are currently encapsulated in HttpRequestException
        }

        [Test]
        public void FetchRemoteRegistryModelDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermojax;1";
            Assert.ThrowsAsync<HttpRequestException>(async () => await _remoteFetcher.FetchAsync(targetDtmi, _remoteUri, _logger.Object));

            // Don't need to verify a log entry as any http errors from fetcher are currently encapsulated in HttpRequestException
        }
    }
}