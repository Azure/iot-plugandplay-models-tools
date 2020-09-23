using Azure.DigitalTwins.Resolver.Fetchers;
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
        IModelFetcher _localFetcher;
        IModelFetcher _remoteFetcher;

        [SetUp]
        public void Setup()
        {
            _localFetcher = new LocalModelFetcher();
            _remoteFetcher = new RemoteModelFetcher();
        }

        [Test]
        public async Task FetchLocalRegistry()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            // Resolution correctness is covered in ResolverIntegrationTests
            string content = await _localFetcher.FetchAsync(targetDtmi, _localUri);
            Assert.IsNotNull(content);
        }

        [Test]
        public void FetchLocalRegistryDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            Uri invalidFileUri = new Uri("file://totally/fake/path/please");
            Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await _localFetcher.FetchAsync(targetDtmi, invalidFileUri));
        }

        [Test]
        public void FetchLocalRegistryModelDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermojax;1";
            Assert.ThrowsAsync<FileNotFoundException>(async () => await _localFetcher.FetchAsync(targetDtmi, _localUri));
        }

        [Test]
        public async Task FetchRemoteRegistry()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";

            // Resolution correctness is covered in ResolverIntegrationTests
            string content = await _remoteFetcher.FetchAsync(targetDtmi, _remoteUri);
            Assert.IsNotNull(content);
        }

        [Test]
        public void FetchRemoteRegistryDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermostat;1";
            Uri invalidRemoteUri = new Uri("http://localhost/fakeRegistry/");
            Assert.ThrowsAsync<HttpRequestException>(async () => await _remoteFetcher.FetchAsync(targetDtmi, invalidRemoteUri));
        }

        [Test]
        public void FetchRemoteRegistryModelDoesNotExist()
        {
            string targetDtmi = "dtmi:com:example:thermojax;1";
            Assert.ThrowsAsync<HttpRequestException>(async () => await _remoteFetcher.FetchAsync(targetDtmi, _remoteUri));
        }
    }
}