using Microsoft.IoT.ModelValidator.Models;
using Microsoft.IoT.ModelValidator.Services;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Threading.Tasks;

namespace Microsoft.IoT.ModelValidator
{
    public enum OutputFormat
    {
        space_delimited,
        csv,
        json
    }

    public enum FileChangeStatus
    {
        added,
        removed,
        modified,
        renamed,
        none
    }

    public class Program
    {
        private static long REPO_ID = 295857906;

        static async Task Main(string[] args)
        {
            string authToken = args[0];
            int pullRequestId = int.Parse(args[1]);
            string format = args[2];

            if (!Enum.IsDefined(typeof(OutputFormat), format))
            {
                throw new Exception($"Format must be one of 'space_delimited', 'csv', or 'json', got '{format}'.");
            }

            OutputFormat outputFormat = (OutputFormat)Enum.Parse(typeof(OutputFormat), format);

            GitHubClient gitClient = new GitHubClient(new ProductHeaderValue("model-validator"));
            gitClient.Credentials = new Credentials(authToken);

            ModelValidationService modelValidationService = new ModelValidationService(gitClient);

            RepositoryUpdatesFormatted result = await modelValidationService.GetRepositoryUpdates(REPO_ID, pullRequestId, outputFormat);

            Console.WriteLine($"::set-output name=all::{result.FilesAllFormatted}");
            Console.WriteLine($"::set-output name=added::{result.FilesAddedFormatted}");
            Console.WriteLine($"::set-output name=modified::{result.FilesModifiedFormatted}");
            Console.WriteLine($"::set-output name=removed::{result.FilesRemovedFormatted}");
            Console.WriteLine($"::set-output name=renamed::{result.FilesRenamedFormatted}");
            Console.WriteLine($"::set-output name=added_modified::{result.FilesAddedModifiedFormatted}");
        }
    }
}
