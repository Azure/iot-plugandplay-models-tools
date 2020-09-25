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
        ClientLogger _clientLogger;
        Mock<ILogger> _logger;
        IModelFetcher _localFetcher;
        IModelFetcher _remoteFetcher;

        [SetUp]
        public void Setup()
        {
            _localFetcher = new LocalModelFetcher();
            _remoteFetcher = new RemoteModelFetcher();
            _logger = Mocks.GetGenericILogger();
            _clientLogger = new ClientLogger(_logger.Object);
        }

        [Test]
        public async Task FetchLocalRegistry()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            // Resolution correctness is covered in ResolverIntegrationTests
            string content = await _localFetcher.FetchAsync(targetDtmi, _localUri, _clientLogger);
            Assert.IsNotNull(content);
            _logger.Verify();
        }

        [Test]
        public void FetchLocalRegistryDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            Uri invalidFileUri = new Uri("file://totally/fake/path/please");
            Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await _localFetcher.FetchAsync(targetDtmi, invalidFileUri, _clientLogger));
            _logger.Verify();
        }

        [Test]
        public void FetchLocalRegistryModelDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermojax;1";
            Assert.ThrowsAsync<FileNotFoundException>(async () => await _localFetcher.FetchAsync(targetDtmi, _localUri, _clientLogger));
            _logger.Verify();
        }

        [Test]
        public async Task FetchRemoteRegistry()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            // Resolution correctness is covered in ResolverIntegrationTests
            string content = await _remoteFetcher.FetchAsync(targetDtmi, _remoteUri, _clientLogger);
            Assert.IsNotNull(content);
            _logger.Verify();
        }

        [Test]
        public void FetchRemoteRegistryDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";
            Uri invalidRemoteUri = new Uri("http://localhost/fakeRegistry/");
            Assert.ThrowsAsync<HttpRequestException>(async () => await _remoteFetcher.FetchAsync(targetDtmi, invalidRemoteUri, _clientLogger));
            _logger.Verify();
        }

        [Test]
        public void FetchRemoteRegistryModelDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermojax;1";
            Assert.ThrowsAsync<HttpRequestException>(async () => await _remoteFetcher.FetchAsync(targetDtmi, _remoteUri, _clientLogger));
            _logger.Verify();
        }
    }
}