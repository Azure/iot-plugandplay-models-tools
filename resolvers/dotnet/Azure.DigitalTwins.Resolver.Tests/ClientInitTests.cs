using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;

namespace Azure.DigitalTwins.Resolver.Tests
{
    public class ClientInitTests
    {
        Mock<ILogger> _logger;

        [SetUp]
        public void Setup()
        {
            _logger = Mocks.GetGenericILogger();
        }

        [Test]
        public void ClientInitGenericRegistryUri()
        {
            string registryUriString = "https://localhost/myregistry/";
            Uri registryUri = new Uri(registryUriString);
            var client = new ResolverClient(registryUri);
            Assert.AreEqual(registryUri, client.RegistryUri);
        }

        [Test]
        public void ClientInitRemoteRegistryHelper()
        {
            string registryUriString = "https://localhost/myregistry/";
            Uri registryUri = new Uri(registryUriString);
            var client = ResolverClient.FromRemoteRegistry(registryUriString);
            Assert.AreEqual(registryUri, client.RegistryUri);
        }

        [Test]
        public void ClientInitGenericRegistryUriWithLogger()
        {
            string registryUriString = "https://localhost/myregistry/";
            Uri registryUri = new Uri(registryUriString);
            var client = new ResolverClient(registryUri, _logger.Object);
            Assert.AreEqual(registryUri, client.RegistryUri);
            _logger.Verify();
        }

        [Test]
        public void ClientInitRemoteRegistryHelperWithLogger()
        {
            string registryUriString = "https://localhost/myregistry/";
            Uri registryUri = new Uri(registryUriString);
            var client = ResolverClient.FromRemoteRegistry(registryUriString, _logger.Object);
            Assert.AreEqual(registryUri, client.RegistryUri);
            _logger.Verify();
        }

        [Test]
        public void ClientInitLocalRegistryHelper()
        {
            string registryPathWindows = @"C:\Users\me\path\to\registy";
            string registryPathLinux = "/me/path/to/registry";

            Uri registryUriWindows = new Uri($"file://{registryPathWindows}");
            var clientWindows = ResolverClient.FromLocalRegistry(registryPathWindows, _logger.Object);

            Assert.AreEqual(registryUriWindows, clientWindows.RegistryUri);
            Assert.AreEqual(registryPathWindows.Replace('\\', '/'), clientWindows.RegistryUri.AbsolutePath);
            _logger.Verify();

            Uri registryUriLinux = new Uri($"file://{registryPathLinux}");
            var clientLinux = ResolverClient.FromLocalRegistry(registryPathLinux);

            Assert.AreEqual(registryUriLinux, clientLinux.RegistryUri);
            Assert.AreEqual(registryPathLinux, clientLinux.RegistryUri.AbsolutePath);
        }
    }
}