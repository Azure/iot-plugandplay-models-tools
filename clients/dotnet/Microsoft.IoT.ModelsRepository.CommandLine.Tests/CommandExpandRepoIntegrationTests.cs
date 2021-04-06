using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Microsoft.IoT.ModelsRepository.CommandLine.Tests
{
    [NonParallelizable]
    public class CommandExpandRepoIntegrationTests
    {
        readonly string testDirectory = TestContext.CurrentContext.TestDirectory;
        DirectoryInfo testExpandableRepo;

        [OneTimeSetUp]
        public void ResetTestRepoDir()
        {
            testExpandableRepo = new DirectoryInfo(Path.Combine(testDirectory, "MyExpandableModelRepo"));
            if (testExpandableRepo.Exists && testExpandableRepo.Name == "MyExpandableModelRepo")
            {
                testExpandableRepo.Delete(true);
            }
        }

        [TestCase]
        public void ExpandModelsRepo()
        {
            TestHelpers.DirectoryCopy(
                $"{Path.Combine(TestHelpers.TestLocalModelRepository, "indexable")}",
                testExpandableRepo.FullName, true, true);

            (int returnCode, string _, string standardError) =
                ClientInvokator.Invoke($"expand --local-repo {testExpandableRepo.FullName}");

            Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);
            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));

            var modelFilePaths = new List<string>();
            foreach (string file in Directory.EnumerateFiles(testExpandableRepo.FullName, "*.json",
                new EnumerationOptions { RecurseSubdirectories = true }))
            {
                if (file.EndsWith(".expanded.json"))
                {
                    continue;
                }
                modelFilePaths.Add(file);
            }

            foreach (string modelFile in modelFilePaths)
            {
                string expandedModelFile = modelFile.Replace(".json", ".expanded.json");
                using JsonDocument expandedModelDocument = JsonDocument.Parse(File.ReadAllText(expandedModelFile));
                JsonElement root = expandedModelDocument.RootElement;
                Assert.AreEqual(root.ValueKind, JsonValueKind.Array);
                JsonElement firstModelElement = root[0];
                string modelFileContent = File.ReadAllText(modelFile);
                Assert.AreEqual(ParsingUtils.GetRootId(modelFileContent), firstModelElement.GetProperty("@id").GetString());
            }
        }

        [TestCase]
        public void ExpandModelsRepoSilentNoStandardOut()
        {
            TestHelpers.DirectoryCopy(
                $"{Path.Combine(TestHelpers.TestLocalModelRepository, "indexable")}",
                testExpandableRepo.FullName, true, true);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"expand --local-repo {testExpandableRepo.FullName} --silent");

            Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);
            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.AreEqual(string.Empty, standardOut);
        }

        [TestCase]
        public void ExpandModelsRepoSupportsDebugHeaders()
        {
            testExpandableRepo.Create();
            (int returnCode, string _, string standardError) =
                ClientInvokator.Invoke($"expand --local-repo {testExpandableRepo.FullName} --debug");

            Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);
            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardError.Contains(Outputs.DebugHeader));
        }

        [TestCase]
        public void ExpandModelsRepoWillErrorWithInvalidDirectory()
        {
            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"expand --local-repo ./nonexistent_directory/");

            Assert.AreEqual(Handlers.ReturnCodes.InvalidArguments, returnCode);
            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.AreEqual(string.Empty, standardOut);
        }

        [TestCase]
        public void ExpandModelsWillErrorsOnInvalidModelJson()
        {
            (int returnCode, string _, string standardError) =
                ClientInvokator.Invoke($"expand");
            Assert.AreEqual(Handlers.ReturnCodes.ProcessingError, returnCode);
            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
        }
    }
}
