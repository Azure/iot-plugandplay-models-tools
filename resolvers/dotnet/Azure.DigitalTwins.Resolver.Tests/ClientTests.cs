using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Azure.DigitalTwins.Resolver.Tests
{
    public class ClientTests
    {
        Mock<ILogger> _logger;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger>();
        }

        [Test]
        public void ClientInitGenericRegistryUri()
        {
            string registryUriString = "https://localhost/myregistry/";
            Uri registryUri = new Uri(registryUriString);

            // Uses NullLogger
            var client = new ResolverClient(registryUri);
            Assert.AreEqual(registryUri, client.RegistryUri);

            client = new ResolverClient(registryUri, _logger.Object);
            Assert.AreEqual(registryUri, client.RegistryUri);
            _logger.ValidateLog("Client initialized with http content fetcher.", LogLevel.Information, Times.Once());
        }

        [Test]
        public void ClientInitRemoteRegistryHelper()
        {
            string registryUriString = "https://localhost/myregistry/";
            Uri registryUri = new Uri(registryUriString);

            var client = ResolverClient.FromRemoteRegistry(registryUriString);
            Assert.AreEqual(registryUri, client.RegistryUri);

            client = ResolverClient.FromRemoteRegistry(registryUriString, _logger.Object);
            Assert.AreEqual(registryUri, client.RegistryUri);
            _logger.ValidateLog("Client initialized with http content fetcher.", LogLevel.Information, Times.Once());
        }

        [Test]
        public void ClientInitLocalRegistryHelper()
        {
            string testModelRegistryPath = TestHelpers.GetTestLocalModelRegistry();
            Uri registryUri = new Uri($"file://{testModelRegistryPath}");

            // Uses NullLogger
            var client = ResolverClient.FromLocalRegistry(testModelRegistryPath);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                testModelRegistryPath = testModelRegistryPath.Replace("\\", "/");
            }

            Assert.AreEqual(registryUri, client.RegistryUri);
            Assert.AreEqual(testModelRegistryPath, client.RegistryUri.AbsolutePath);

            client = ResolverClient.FromLocalRegistry(testModelRegistryPath, _logger.Object);
            Assert.AreEqual(registryUri, client.RegistryUri);

            _logger.ValidateLog("Client initialized with file content fetcher.", LogLevel.Information, Times.Once());
        }

        [TestCase("dtmi:com:example:Thermostat;1", true)]
        [TestCase("dtmi:contoso:scope:entity;2", true)]
        [TestCase("dtmi:com:example:Thermostat:1", false)]
        [TestCase("dtmi:com:example::Thermostat;1", false)]
        [TestCase("com:example:Thermostat;1", false)]
        public void ClientIsValidDtmi(string dtmi, bool expected)
        {
            Assert.AreEqual(ResolverClient.IsValidDtmi(dtmi), expected);
        }
    }
}
