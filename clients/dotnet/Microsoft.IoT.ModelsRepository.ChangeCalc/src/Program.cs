using Microsoft.IoT.ModelsRepository.ChangeCalc.Models;
using Microsoft.IoT.ModelsRepository.ChangeCalc.Services;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Microsoft.IoT.ModelsRepository.ChangeCalc.Tests")]
namespace Microsoft.IoT.ModelsRepository.ChangeCalc
{
    enum OutputFormat
    {
        space_delimited,
        csv,
        json
    }

    enum FileChangeStatus
    {
        added,
        removed,
        modified,
        renamed,
        none
    }

    public class Program
    {
        static async Task Main(string[] args)
        {
            string authToken = args[0];
            long repoId = long.Parse(args[1]);
            int pullRequestId = int.Parse(args[2]);
            string format = args[3];

            if (!Enum.IsDefined(typeof(OutputFormat), format))
            {
                throw new Exception($"Format must be one of 'space_delimited', 'csv', or 'json', got '{format}'.");
            }

            OutputFormat outputFormat = (OutputFormat)Enum.Parse(typeof(OutputFormat), format);

            GitHubClient gitClient = new GitHubClient(new ProductHeaderValue("Microsoft.IoT.ModelsRepository.ChangeCalc"));
            gitClient.Credentials = new Credentials(authToken);

            IModelValidationService modelValidationService = new ModelValidationService(gitClient);

            RepositoryUpdatesFormatted result = await modelValidationService.GetRepositoryUpdates(repoId, pullRequestId, outputFormat);

            Console.WriteLine($"::set-output name=all::{result.FilesAllFormatted}");
            Console.WriteLine($"::set-output name=added::{result.FilesAddedFormatted}");
            Console.WriteLine($"::set-output name=modified::{result.FilesModifiedFormatted}");
            Console.WriteLine($"::set-output name=removed::{result.FilesRemovedFormatted}");
            Console.WriteLine($"::set-output name=renamed::{result.FilesRenamedFormatted}");
            Console.WriteLine($"::set-output name=added_modified::{result.FilesAddedModifiedFormatted}");
        }
    }
}
