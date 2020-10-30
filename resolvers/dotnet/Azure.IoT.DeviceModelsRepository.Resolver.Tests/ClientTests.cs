using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace Azure.IoT.DeviceModelsRepository.Resolver.Tests
{
    public class ClientTests
    {
        [Test]
        public void ClientInitRemoteRepository()
        {
            Mock<ILogger> fromUriLogger = new Mock<ILogger>();
            Mock<ILogger> fromStrLogger = new Mock<ILogger>();

            string remoteRepoEndpoint = "https://localhost/repository";
            Uri repositoryUri = new Uri(remoteRepoEndpoint);

            ResolverClient clientInitUri = new ResolverClient(repositoryUri, default, fromUriLogger.Object);
            ResolverClient clientInitStr = new ResolverClient(remoteRepoEndpoint, default, logger: fromStrLogger.Object);

            Assert.AreEqual(repositoryUri, clientInitUri.RepositoryUri);
            Assert.AreEqual(remoteRepoEndpoint, clientInitUri.RepositoryUri.AbsoluteUri);

            fromUriLogger.ValidateLog(StandardStrings.ClientInitWithFetcher(repositoryUri.Scheme), LogLevel.Trace, Times.Once());

            Assert.AreEqual(repositoryUri, clientInitStr.RepositoryUri);
            Assert.AreEqual(remoteRepoEndpoint, clientInitStr.RepositoryUri.AbsoluteUri);

            fromStrLogger.ValidateLog(StandardStrings.ClientInitWithFetcher(repositoryUri.Scheme), LogLevel.Trace, Times.Once());

            ResolverClient clientInitDefault = new ResolverClient();
            Assert.AreEqual($"{ResolverClient.DefaultRepository}/", clientInitDefault.RepositoryUri.AbsoluteUri);
        }

        [Test]
        public void ClientInitLocalRepository()
        {
            Mock<ILogger> fromUriLogger = new Mock<ILogger>();
            Mock<ILogger> fromStrLogger = new Mock<ILogger>();

            string localTestRepoPath = TestHelpers.TestLocalModelRepository;
            Uri repositoryUri = new Uri($"file://{localTestRepoPath}");

            ResolverClient clientInitUri = new ResolverClient(repositoryUri, default, fromUriLogger.Object) ;
            ResolverClient clientInitStr = new ResolverClient(localTestRepoPath, default, logger: fromStrLogger.Object);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                localTestRepoPath = localTestRepoPath.Replace("\\", "/");
            }

            Assert.AreEqual(repositoryUri, clientInitUri.RepositoryUri);
            Assert.AreEqual(localTestRepoPath, clientInitUri.RepositoryUri.AbsolutePath);

            fromUriLogger.ValidateLog(StandardStrings.ClientInitWithFetcher(repositoryUri.Scheme), LogLevel.Trace, Times.Once());

            Assert.AreEqual(repositoryUri, clientInitStr.RepositoryUri);
            Assert.AreEqual(localTestRepoPath, clientInitStr.RepositoryUri.AbsolutePath);

            fromStrLogger.ValidateLog(StandardStrings.ClientInitWithFetcher(repositoryUri.Scheme), LogLevel.Trace, Times.Once());
        }

        [TestCase("dtmi:com:example:Thermostat;1", true)]
        [TestCase("dtmi:contoso:scope:entity;2", true)]
        [TestCase("dtmi:com:example:Thermostat:1", false)]
        [TestCase("dtmi:com:example::Thermostat;1", false)]
        [TestCase("com:example:Thermostat;1", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void ClientIsValidDtmi(string dtmi, bool expected)
        {
            Assert.AreEqual(ResolverClient.IsValidDtmi(dtmi), expected);
        }

        [Test]
        public void ClientOptions()
        {
            DependencyResolutionOption defaultResolutionOption = DependencyResolutionOption.Enabled;
            ResolverClientOptions customOptions = 
                new ResolverClientOptions(DependencyResolutionOption.TryFromExpanded);

            string repositoryUriString = "https://localhost/myregistry/";
            Uri repositoryUri = new Uri(repositoryUriString);

            ResolverClient defaultClient = new ResolverClient(repositoryUri);
            Assert.AreEqual(defaultClient.ClientOptions.DependencyResolution, defaultResolutionOption);

            ResolverClient customClient = new ResolverClient(repositoryUriString, customOptions);
            Assert.AreEqual(customClient.ClientOptions.DependencyResolution, DependencyResolutionOption.TryFromExpanded);
        }

        [Test]
        public void CtorOverloads()
        {
            var uri = new Uri("https://dtmi.com");
            ILogger logger = new NullLogger<ClientTests>();
            var options = new ResolverClientOptions();

            Assert.AreEqual(new Uri(ResolverClient.DefaultRepository), new ResolverClient().RepositoryUri);
            Assert.AreEqual(new Uri(ResolverClient.DefaultRepository), new ResolverClient(logger).RepositoryUri);
            Assert.AreEqual(new Uri(ResolverClient.DefaultRepository), new ResolverClient(options).RepositoryUri);
            Assert.AreEqual(new Uri(ResolverClient.DefaultRepository), new ResolverClient(options,logger).RepositoryUri);

            Assert.AreEqual(uri, new ResolverClient(uri).RepositoryUri);
            Assert.AreEqual(uri, new ResolverClient(uri, options).RepositoryUri);
            Assert.AreEqual(uri, new ResolverClient(uri, null, logger).RepositoryUri);
            Assert.AreEqual(uri, new ResolverClient(uri, options, null).RepositoryUri);
            Assert.AreEqual(uri, new ResolverClient(uri, options, logger).RepositoryUri);
        }
    }
}

