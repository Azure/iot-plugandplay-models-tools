using System;
using Xunit;
using Octokit;
using Moq;
using System.Collections.Generic;
using Microsoft.IoT.ModelsRepository.ChangeCalc.Services;
using Microsoft.IoT.ModelsRepository.ChangeCalc.Models;

namespace Microsoft.IoT.ModelsRepository.ChangeCalc.Tests
{
    class TestPullRequestFile : PullRequestFile
    {
        internal TestPullRequestFile(string fileName, string status)
        {
            FileName = fileName;
            Status = status;
        }
    }

    public class RepositoryUpdatesFetchAndFormatTests
    {
        private const string repositoryOwner = "Azure";
        private const string repositoryName = "iot-plugandplay-modelz";
        private const int pullRequestId = 0;

        private static Mock<IGitHubClient> GitClientMoq = new Mock<IGitHubClient>();

        private IModelValidationService ModelValidationService = new ModelValidationService(GitClientMoq.Object);

        List<PullRequestFile> ModelRepositoryFileUpdates = new List<PullRequestFile>
        {
            new TestPullRequestFile("dtmi/test_company_1/test_device_1_interface_1.json", "added"),
            new TestPullRequestFile("dtmi/test_company_1/test_device_1_interface_2.json", "modified"),
            new TestPullRequestFile("dtmi/test_company_1/test_device_2_interface_1.json", "removed"),
            new TestPullRequestFile("dtmi/test_company_1/test_device_2_interface_2.json", "added"),
            new TestPullRequestFile("dtmi/test_company_1/test_device_3_interface_1.json", "renamed"),
            new TestPullRequestFile("dtmi/test_company_1/test_device_3_interface_2.json", "modified"),
            new TestPullRequestFile("dtmi/test_company_1/test_device_3_interface_3.json", "none")
        };

        [Fact]
        public async void GetRepositoryUpdates_ResultsFormatted_Csv()
        {
            GitClientMoq.Setup(client => client.PullRequest.Files(repositoryOwner, repositoryName, pullRequestId)).ReturnsAsync(ModelRepositoryFileUpdates);

            RepositoryUpdatesFormatted expectedRepoUpdates = new RepositoryUpdatesFormatted
            {
                FilesAddedFormatted = "dtmi/test_company_1/test_device_1_interface_1.json,dtmi/test_company_1/test_device_2_interface_2.json",
                FilesModifiedFormatted = "dtmi/test_company_1/test_device_1_interface_2.json,dtmi/test_company_1/test_device_3_interface_2.json",
                FilesRemovedFormatted = "dtmi/test_company_1/test_device_2_interface_1.json",
                FilesRenamedFormatted = "dtmi/test_company_1/test_device_3_interface_1.json",
                FilesAddedModifiedFormatted = "dtmi/test_company_1/test_device_1_interface_1.json,dtmi/test_company_1/test_device_1_interface_2.json,dtmi/test_company_1/test_device_2_interface_2.json,dtmi/test_company_1/test_device_3_interface_2.json",
                FilesAllFormatted = "dtmi/test_company_1/test_device_1_interface_1.json,dtmi/test_company_1/test_device_1_interface_2.json,dtmi/test_company_1/test_device_2_interface_1.json,dtmi/test_company_1/test_device_2_interface_2.json,dtmi/test_company_1/test_device_3_interface_1.json,dtmi/test_company_1/test_device_3_interface_2.json,dtmi/test_company_1/test_device_3_interface_3.json"
            };

            RepositoryUpdatesFormatted actualRepoUpdates = await ModelValidationService.GetRepositoryUpdates(repositoryOwner, repositoryName, pullRequestId, OutputFormat.csv);

            AssertRepoUpdates(expectedRepoUpdates, actualRepoUpdates);
        }

        [Fact]
        public async void GetRepositoryUpdates_ResultsFormatted_SpaceDelimited()
        {
            GitClientMoq.Setup(client => client.PullRequest.Files(repositoryOwner, repositoryName, pullRequestId)).ReturnsAsync(ModelRepositoryFileUpdates);

            RepositoryUpdatesFormatted expectedRepoUpdates = new RepositoryUpdatesFormatted
            {
                FilesAddedFormatted = "dtmi/test_company_1/test_device_1_interface_1.json dtmi/test_company_1/test_device_2_interface_2.json",
                FilesModifiedFormatted = "dtmi/test_company_1/test_device_1_interface_2.json dtmi/test_company_1/test_device_3_interface_2.json",
                FilesRemovedFormatted = "dtmi/test_company_1/test_device_2_interface_1.json",
                FilesRenamedFormatted = "dtmi/test_company_1/test_device_3_interface_1.json",
                FilesAddedModifiedFormatted = "dtmi/test_company_1/test_device_1_interface_1.json dtmi/test_company_1/test_device_1_interface_2.json dtmi/test_company_1/test_device_2_interface_2.json dtmi/test_company_1/test_device_3_interface_2.json",
                FilesAllFormatted = "dtmi/test_company_1/test_device_1_interface_1.json dtmi/test_company_1/test_device_1_interface_2.json dtmi/test_company_1/test_device_2_interface_1.json dtmi/test_company_1/test_device_2_interface_2.json dtmi/test_company_1/test_device_3_interface_1.json dtmi/test_company_1/test_device_3_interface_2.json dtmi/test_company_1/test_device_3_interface_3.json"
            };

            RepositoryUpdatesFormatted actualRepoUpdates = await ModelValidationService.GetRepositoryUpdates(repositoryOwner, repositoryName, pullRequestId, OutputFormat.space_delimited);

            AssertRepoUpdates(expectedRepoUpdates, actualRepoUpdates);
        }

        [Fact]
        public async void GetRepositoryUpdates_ResultsFormatted_Json()
        {
            GitClientMoq.Setup(client => client.PullRequest.Files(repositoryOwner, repositoryName, pullRequestId)).ReturnsAsync(ModelRepositoryFileUpdates);

            RepositoryUpdatesFormatted expectedRepoUpdates = new RepositoryUpdatesFormatted
            {
                FilesAddedFormatted = "[\"dtmi/test_company_1/test_device_1_interface_1.json\",\"dtmi/test_company_1/test_device_2_interface_2.json\"]",
                FilesModifiedFormatted = "[\"dtmi/test_company_1/test_device_1_interface_2.json\",\"dtmi/test_company_1/test_device_3_interface_2.json\"]",
                FilesRemovedFormatted = "[\"dtmi/test_company_1/test_device_2_interface_1.json\"]",
                FilesRenamedFormatted = "[\"dtmi/test_company_1/test_device_3_interface_1.json\"]",
                FilesAddedModifiedFormatted = "[\"dtmi/test_company_1/test_device_1_interface_1.json\",\"dtmi/test_company_1/test_device_1_interface_2.json\",\"dtmi/test_company_1/test_device_2_interface_2.json\",\"dtmi/test_company_1/test_device_3_interface_2.json\"]",
                FilesAllFormatted = "[\"dtmi/test_company_1/test_device_1_interface_1.json\",\"dtmi/test_company_1/test_device_1_interface_2.json\",\"dtmi/test_company_1/test_device_2_interface_1.json\",\"dtmi/test_company_1/test_device_2_interface_2.json\",\"dtmi/test_company_1/test_device_3_interface_1.json\",\"dtmi/test_company_1/test_device_3_interface_2.json\",\"dtmi/test_company_1/test_device_3_interface_3.json\"]"
            };

            RepositoryUpdatesFormatted actualRepoUpdates = await ModelValidationService.GetRepositoryUpdates(repositoryOwner, repositoryName, pullRequestId, OutputFormat.json);

            AssertRepoUpdates(expectedRepoUpdates, actualRepoUpdates);
        }

        [Fact]
        public async void GetRepositoryUpdates_HandlesEmptyPullRequestFiles()
        {
            GitClientMoq.Setup(client => client.PullRequest.Files(repositoryOwner, repositoryName, pullRequestId)).ReturnsAsync(new List<PullRequestFile>());

            RepositoryUpdatesFormatted expectedRepoUpdates = new RepositoryUpdatesFormatted
            {
                FilesAddedFormatted = "",
                FilesModifiedFormatted = "",
                FilesRemovedFormatted = "",
                FilesRenamedFormatted = "",
                FilesAddedModifiedFormatted = "",
                FilesAllFormatted = ""
            };

            RepositoryUpdatesFormatted actualRepoUpdates = await ModelValidationService.GetRepositoryUpdates(repositoryOwner, repositoryName, pullRequestId, OutputFormat.space_delimited);

            AssertRepoUpdates(expectedRepoUpdates, actualRepoUpdates);
        }

        [Fact]
        public async void GetRepositoryUpdates_ThrowsExceptionWhenSpaceInFile_SpaceDelimited()
        {
            var repoUpdatesWithSpace = new List<PullRequestFile>
            {
                new TestPullRequestFile("dtmi/test_company_1/test_device_1 interface_1.json", "added"),
            };

            GitClientMoq.Setup(client => client.PullRequest.Files(repositoryOwner, repositoryName, pullRequestId)).ReturnsAsync(repoUpdatesWithSpace);

            await Assert.ThrowsAsync<Exception>(() => ModelValidationService.GetRepositoryUpdates(repositoryOwner, repositoryName, pullRequestId, OutputFormat.space_delimited));
        }

        [Fact]
        public async void GetRepositoryUpdates_ThrowsExceptionWhenFileStatusInvalid()
        {
            var repoUpdatesWithSpace = new List<PullRequestFile>
            {
                new TestPullRequestFile("dtmi/test_company_1/test_device_1_interface_1.json", "random"),
            };

            GitClientMoq.Setup(client => client.PullRequest.Files(repositoryOwner, repositoryName, pullRequestId)).ReturnsAsync(repoUpdatesWithSpace);

            await Assert.ThrowsAsync<ArgumentException>(() => ModelValidationService.GetRepositoryUpdates(repositoryOwner, repositoryName, pullRequestId, OutputFormat.json));
        }

        private void AssertRepoUpdates(RepositoryUpdatesFormatted expectedRepoUpdates, RepositoryUpdatesFormatted actualRepoUpdates)
        {
            Assert.NotNull(actualRepoUpdates);
            Assert.Equal(expectedRepoUpdates.FilesAddedFormatted, actualRepoUpdates.FilesAddedFormatted);
            Assert.Equal(expectedRepoUpdates.FilesModifiedFormatted, actualRepoUpdates.FilesModifiedFormatted);
            Assert.Equal(expectedRepoUpdates.FilesRemovedFormatted, actualRepoUpdates.FilesRemovedFormatted);
            Assert.Equal(expectedRepoUpdates.FilesRenamedFormatted, actualRepoUpdates.FilesRenamedFormatted);
            Assert.Equal(expectedRepoUpdates.FilesAddedModifiedFormatted, actualRepoUpdates.FilesAddedModifiedFormatted);
            Assert.Equal(expectedRepoUpdates.FilesAllFormatted, actualRepoUpdates.FilesAllFormatted);
        }
    }
}
