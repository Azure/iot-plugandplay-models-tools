using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Microsoft.IoT.ModelsRepository.CommandLine.Tests
{
    [NonParallelizable]
    public class CommandIndexIntegrationTests
    {
        string indexableRepoPath = string.Empty;

        [OneTimeSetUp]
        public void InitializeIndexTests()
        {
            indexableRepoPath = $"{Path.Combine(TestHelpers.TestLocalModelRepository, "indexable")}";
        }

        // TODO: Consider paging strategy.
        [TestCase("./dmr-index.json")]
        public void IndexModels(string outfilePath)
        {
            outfilePath = Path.GetFullPath(outfilePath);
            string outfileArg = $"-o {outfilePath}";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"index --local-repo {indexableRepoPath} {outfileArg}");

            Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);
            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));

            var expectedIndexEntry = new List<ModelIndexEntry>();
            foreach (string file in Directory.EnumerateFiles(indexableRepoPath, "*.json",
                new EnumerationOptions { RecurseSubdirectories = true }))
            {
                if (file.ToLower().EndsWith(".expanded.json"))
                {
                    continue;
                }

                expectedIndexEntry.Add(ParsingUtils.ParseModelFileForIndex(new FileInfo(file)));
            }

            string indexJson = File.ReadAllText(outfilePath);

            using JsonDocument document = JsonDocument.Parse(indexJson);
            JsonElement root = document.RootElement;
            Assert.AreEqual(root.ValueKind, JsonValueKind.Object);

            foreach (ModelIndexEntry entry in expectedIndexEntry)
            {
                JsonElement dtmiElement = root.GetProperty(entry.Dtmi);
                if (entry.Description != null)
                {
                    /// System.Text.Json does not currently support deep object comparison.
                    string expectedDescJson = JsonSerializer.Serialize(entry.Description);
                    Assert.AreEqual(expectedDescJson, JsonSerializer.Serialize(dtmiElement.GetProperty("description")));
                }
                if (entry.DisplayName != null)
                {
                    /// System.Text.Json does not currently support deep object comparison.
                    string expectedDisplayNameJson = JsonSerializer.Serialize(entry.DisplayName);
                    Assert.AreEqual(expectedDisplayNameJson, JsonSerializer.Serialize(dtmiElement.GetProperty("displayName")));
                }
            }
        }

        [TestCase("./dmr-index.json")]
        public void IndexModelsSupportsDebugHeaders(string outfilePath)
        {
            outfilePath = Path.GetFullPath(outfilePath);
            (int returnCode, string _, string standardError) =
                ClientInvokator.Invoke($"index --local-repo {indexableRepoPath} -o {outfilePath} --debug");
            Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);
            Assert.True(standardError.Contains(Outputs.DebugHeader));
        }

        [TestCase("./dmr-index.json")]
        public void IndexModelsSilentNoStandardOut(string outfilePath)
        {
            outfilePath = Path.GetFullPath(outfilePath);
            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"index --local-repo {indexableRepoPath} -o {outfilePath} --silent");
            Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);
            Assert.True(string.IsNullOrEmpty(standardOut));
        }

        [TestCase]
        public void IndexModelsErrorsOnInvalidModelJson()
        {
            (int returnCode, string _, string standardError) =
                ClientInvokator.Invoke($"index -o willfail.json");
            Assert.AreEqual(Handlers.ReturnCodes.ProcessingError, returnCode);
            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
        }

        [TestCase]
        public void IndexModelsWillErrorWithInvalidDirectory()
        {
            (int returnCode, string _, string standardError) =
                ClientInvokator.Invoke($"index --local-repo ./nonexistent_directory/");
            Assert.AreEqual(Handlers.ReturnCodes.InvalidArguments, returnCode);
            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
        }
    }
}
