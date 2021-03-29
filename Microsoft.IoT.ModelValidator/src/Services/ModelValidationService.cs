using Microsoft.IoT.ModelValidator.Models;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.IoT.ModelValidator.Services
{
    public interface IModelValidationService
    {
        Task<RepositoryUpdatesFormatted> GetRepositoryUpdates(long repoId, int pullRequestId, OutputFormat outputFormat);
    }

    public class ModelValidationService : IModelValidationService
    {
        private IGitHubClient gitClient;
        public ModelValidationService(IGitHubClient gitClient)
        {
            this.gitClient = gitClient;
        }
        public async Task<RepositoryUpdatesFormatted> GetRepositoryUpdates(long repoId, int pullRequestId, OutputFormat outputFormat)
        {
            IEnumerable<PullRequestFile> allFiles = await gitClient.PullRequest.Files(repoId, pullRequestId);

            var repoUpdates = new RepositoryUpdates
            {
                FilesAdded = new List<string>(),                                                                                                                                                                                             
                FilesModified = new List<string>(),
                FilesRemoved = new List<string>(),
                FilesRenamed = new List<string>(),
                FilesAddedModified = new List<string>(),
                FilesAll = new List<string>()
            };

            foreach (PullRequestFile file in allFiles)
            {
                if (outputFormat == OutputFormat.space_delimited && file.FileName.Contains(" "))
                {
                    throw new Exception("One of your files includes a space. Consider using a different output format or removing spaces from your filenames. Please submit an issue on this action's GitHub repo.");
                }

                repoUpdates.FilesAll.Add(file.FileName);

                FileChangeStatus fileStatus = (FileChangeStatus)System.Enum.Parse(typeof(FileChangeStatus), file.Status);

                switch (fileStatus)
                {
                    case FileChangeStatus.added:
                        repoUpdates.FilesAdded.Add(file.FileName);
                        repoUpdates.FilesAddedModified.Add(file.FileName);
                        break;

                    case FileChangeStatus.modified:
                        repoUpdates.FilesModified.Add(file.FileName);
                        repoUpdates.FilesAddedModified.Add(file.FileName);
                        break;

                    case FileChangeStatus.removed:
                        repoUpdates.FilesRemoved.Add(file.FileName);
                        break;

                    case FileChangeStatus.renamed:
                        repoUpdates.FilesRenamed.Add(file.FileName);
                        break;

                    case FileChangeStatus.none:
                        continue;

                    default:
                        throw new Exception("Invalid file status");
                }
            }

            var formattedRepoUpdates = FormatRepositoryUpdates(repoUpdates, outputFormat);

            return formattedRepoUpdates;
        }

        private RepositoryUpdatesFormatted FormatRepositoryUpdates(RepositoryUpdates repoUpdates, OutputFormat outputFormat)
        {
            switch (outputFormat)
            {
                case OutputFormat.space_delimited:
                    return new RepositoryUpdatesFormatted
                    {
                        FilesAddedFormatted = string.Join(' ', repoUpdates.FilesAdded),
                        FilesModifiedFormatted = string.Join(' ', repoUpdates.FilesModified),
                        FilesAddedModifiedFormatted = string.Join(' ', repoUpdates.FilesAddedModified),
                        FilesRemovedFormatted = string.Join(' ', repoUpdates.FilesRemoved),
                        FilesRenamedFormatted = string.Join(' ', repoUpdates.FilesRenamed),
                        FilesAllFormatted = string.Join(' ', repoUpdates.FilesAll)
                    };

                case OutputFormat.csv:
                    return new RepositoryUpdatesFormatted
                    {
                        FilesAddedFormatted = string.Join(',', repoUpdates.FilesAdded),
                        FilesModifiedFormatted = string.Join(',', repoUpdates.FilesModified),
                        FilesAddedModifiedFormatted = string.Join(',', repoUpdates.FilesAddedModified),
                        FilesRemovedFormatted = string.Join(',', repoUpdates.FilesRemoved),
                        FilesRenamedFormatted = string.Join(',', repoUpdates.FilesRenamed),
                        FilesAllFormatted = string.Join(',', repoUpdates.FilesAll)
                    };

                case OutputFormat.json:
                    return new RepositoryUpdatesFormatted
                    {
                        FilesAddedFormatted = JsonConvert.SerializeObject(repoUpdates.FilesAdded),
                        FilesModifiedFormatted = JsonConvert.SerializeObject(repoUpdates.FilesModified),
                        FilesAddedModifiedFormatted = JsonConvert.SerializeObject(repoUpdates.FilesAddedModified),
                        FilesRemovedFormatted = JsonConvert.SerializeObject(repoUpdates.FilesRemoved),
                        FilesRenamedFormatted = JsonConvert.SerializeObject(repoUpdates.FilesRenamed),
                        FilesAllFormatted = JsonConvert.SerializeObject(repoUpdates.FilesAll)
                    };

                default:
                    throw new Exception("Invalid output format");
            }
        }
    }
}
