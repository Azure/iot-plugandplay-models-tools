using Microsoft.Extensions.Logging;
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
        public void ClientIsValidDtmi(string dtmi, bool expected)
        {
            Assert.AreEqual(ResolverClient.IsValidDtmi(dtmi), expected);
        }

        [TestCase("dtmi:com:example:Thermostat;1", "/dtmi/com/example/thermostat-1.json", "https://localhost/repository")]
        [TestCase("dtmi:com:example:Thermostat;1", "/dtmi/com/example/thermostat-1.json", null)]
        [TestCase("dtmi:com:example:Thermostat:1", null, "https://localhost/repository")]
        public void ClientGetPath(string dtmi, string expectedPath, string repository)
        {
            if (repository == null)
                repository = TestHelpers.TestLocalModelRepository;

            ResolverClient client = new ResolverClient(repository);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                repository = repository.Replace("\\", "/");
            }

            if (string.IsNullOrEmpty(expectedPath))
            {
                ResolverException re = Assert.Throws<ResolverException>(() => client.GetPath(dtmi));
                Assert.AreEqual(re.Message, $"{StandardStrings.GenericResolverError(dtmi)}{StandardStrings.InvalidDtmiFormat(dtmi)}");
                return;
            }

            string modelPath = client.GetPath(dtmi);
            Assert.AreEqual(modelPath, $"{repository}{expectedPath}");
        }

        [Test]
        public void ClientOptions()
        {
            DependencyResolutionOption defaultResolutionOption = DependencyResolutionOption.Enabled;
            ResolverClientOptions customOptions = 
                new ResolverClientOptions(DependencyResolutionOption.FromExpanded);

            string repositoryUriString = "https://localhost/myregistry/";
            Uri repositoryUri = new Uri(repositoryUriString);

            ResolverClient defaultClient = new ResolverClient(repositoryUri);
            Assert.AreEqual(defaultClient.Settings.DependencyResolution, defaultResolutionOption);

            ResolverClient customClient = new ResolverClient(repositoryUriString, customOptions);
            Assert.AreEqual(customClient.Settings.DependencyResolution, DependencyResolutionOption.FromExpanded);
        }
    }
}
