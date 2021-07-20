// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using NUnit.Framework;
using System;
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

        [TestCase("./index.json", null)]
        [TestCase("./index.json", 2)]
        [TestCase("./super/index.json", 10)]
        [TestCase("./index.json", 0)]
        public void IndexModels(string rootOutfilePath, int? pageLimit)
        {
            rootOutfilePath = Path.GetFullPath(rootOutfilePath);
            var indexDirectory = new FileInfo(rootOutfilePath).Directory.FullName;
            string outfileArg = $"-o {rootOutfilePath}";
            string pageLimitArg = pageLimit.HasValue ? $"--page-limit {pageLimit.Value}" : "";

            (int returnCode, string _, string standardError) =
                ClientInvokator.Invoke($"index --local-repo {indexableRepoPath} {outfileArg} {pageLimitArg}");

            if (pageLimit.HasValue)
            {
                if (pageLimit.Value < 1)
                {
                    Assert.AreEqual(Handlers.ReturnCodes.InvalidArguments, returnCode);
                    return;
                }
            }
            else
            {
                pageLimit = (int)CommonOptions.PageLimit.Argument.GetDefaultValue();
            }

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

            // Process pages
            var work = new Queue<string>();
            work.Enqueue(rootOutfilePath);
            var documentRefs = new List<JsonDocument>();
            var modelRefs = new Dictionary<string, JsonElement>();

            while (work.Count != 0)
            {
                string indexFilePath = work.Dequeue();
                string indexJson = File.ReadAllText(indexFilePath);
                JsonDocument document = JsonDocument.Parse(indexJson);
                documentRefs.Add(document);
                JsonElement root = document.RootElement;
                JsonElement models = root.GetProperty("models");

                if (root.TryGetProperty("links", out JsonElement links))
                {
                    if (links.TryGetProperty("next", out JsonElement next))
                    {
                        string relativeIndexPage = next.GetString();
                        string qualifiedIndexPage = Path.Combine(indexDirectory, relativeIndexPage);
                        work.Enqueue(qualifiedIndexPage);
                    }
                }

                int pageModelCount = 0;
                foreach (var model in models.EnumerateObject())
                {
                    modelRefs.Add(model.Name, model.Value);
                    pageModelCount += 1;
                }

                Assert.LessOrEqual(pageModelCount, pageLimit);
            }

            foreach (ModelIndexEntry entry in expectedIndexEntry)
            {
                JsonElement dtmiElement = modelRefs[entry.Dtmi];
                if (entry.Description != null)
                {
                    // System.Text.Json does not currently support deep object comparison.
                    string expectedDescJson = JsonSerializer.Serialize(entry.Description);
                    Assert.AreEqual(expectedDescJson, JsonSerializer.Serialize(dtmiElement.GetProperty("description")));
                }
                if (entry.DisplayName != null)
                {
                    // System.Text.Json does not currently support deep object comparison.
                    string expectedDisplayNameJson = JsonSerializer.Serialize(entry.DisplayName);
                    Assert.AreEqual(expectedDisplayNameJson, JsonSerializer.Serialize(dtmiElement.GetProperty("displayName")));
                }
            }

            // Assert page count. We are not losing any precision here.
            Assert.AreEqual((int)Math.Ceiling((double)expectedIndexEntry.Count/pageLimit.Value), documentRefs.Count);
            documentRefs.ForEach((doc) => doc.Dispose());
        }

        [TestCase("./index.json")]
        public void IndexModelsSupportsDebugHeaders(string outfilePath)
        {
            outfilePath = Path.GetFullPath(outfilePath);
            (int returnCode, string _, string standardError) =
                ClientInvokator.Invoke($"index --local-repo {indexableRepoPath} -o {outfilePath} --debug");
            Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);
            Assert.True(standardError.Contains(Outputs.DebugHeader));
        }

        [TestCase("./index.json")]
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
