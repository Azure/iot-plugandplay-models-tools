// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Azure.IoT.ModelsRepository;
using NUnit.Framework;

namespace Microsoft.IoT.ModelsRepository.CommandLine.Tests
{
    [NonParallelizable]
    public class CommandImportIntegrationTests
    {
        readonly string testDirectory = TestContext.CurrentContext.TestDirectory;
        DirectoryInfo testImportRepo;

        [OneTimeSetUp]
        public void ResetTestRepoDir()
        {
            testImportRepo = new DirectoryInfo(Path.Combine(testDirectory, "MyImportModelsRepo"));
            if (testImportRepo.Exists && testImportRepo.Name == "MyImportModelsRepo")
            {
                testImportRepo.Delete(true);
            }
        }

        // TODO: Cheap ordering.
        [TestCase("1", "dtmi/com/example/thermostat-1.json", "dtmi:com:example:Thermostat;1")]
        [TestCase("2", "dtmi/azure/devicemanagement/deviceinformation-1.json", "dtmi:azure:DeviceManagement:DeviceInformation;1")]
        [TestCase("3", "dtmi/com/example/temperaturecontroller-1.json", "dtmi:com:example:TemperatureController;1")]
        public void ImportModelFileSingleModelObject(string _, string modelFilePath, string expectedDtmi)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"import --model-file \"{qualifiedModelFilePath}\" {targetRepo}");

            Assert.AreEqual(ReturnCodes.Success, returnCode);
            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));

            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
            Assert.True(standardOut.Contains("* Ensuring DTMIs namespace conformance for model"));
            Assert.True(standardOut.Contains($"* Importing model \"{expectedDtmi}\"."));

            FileInfo modelFile = new FileInfo(Path.GetFullPath(testImportRepo.FullName + "/" + modelFilePath));
            Assert.True(modelFile.Exists);
            DateTime lastWriteTimeUtc = modelFile.LastWriteTimeUtc;
            Assert.AreEqual(expectedDtmi, ParsingUtils.GetRootId(modelFile));

            // Import the same model to ensure its skipped.
            (returnCode, standardOut, _) =
                ClientInvokator.Invoke($"import --model-file \"{qualifiedModelFilePath}\" {targetRepo}");

            Assert.AreEqual(ReturnCodes.Success, returnCode);
            modelFile = new FileInfo(Path.GetFullPath(testImportRepo.FullName + "/" + modelFilePath));
            Assert.AreEqual(lastWriteTimeUtc, modelFile.LastWriteTimeUtc);
            Assert.True(standardOut.Contains($"Skipping \"{expectedDtmi}\". Model file already exists in repository."));

            // Import the same model with --force to ensure its overwritten.
            (returnCode, standardOut, _) =
                ClientInvokator.Invoke($"import --force --model-file \"{qualifiedModelFilePath}\" {targetRepo}");

            Assert.AreEqual(ReturnCodes.Success, returnCode);
            modelFile = new FileInfo(Path.GetFullPath(testImportRepo.FullName + "/" + modelFilePath));
            Assert.True(modelFile.Exists);
            Assert.Less(lastWriteTimeUtc, modelFile.LastWriteTimeUtc);
            Assert.AreEqual(expectedDtmi, ParsingUtils.GetRootId(modelFile));
            Assert.True(standardOut.Contains($"Overriding existing model \"{expectedDtmi}\" because --force option is set."));
        }

        [TestCase(
            "dtmi/com/example/temperaturecontroller-1.expanded.json",
            "dtmi:com:example:TemperatureController;1,dtmi:com:example:Thermostat;1,dtmi:azure:DeviceManagement:DeviceInformation;1",
            "dtmi/com/example/temperaturecontroller-1.json,dtmi/com/example/thermostat-1.json,dtmi/azure/devicemanagement/deviceinformation-1.json")]
        public void ImportModelFileExpandedModelArray(string modelFilePath, string expectedDeps, string expectedPaths)
        {
            // TODO: Revisit.
            //ResetTestRepoDir();

            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"import --model-file \"{qualifiedModelFilePath}\" {targetRepo}");

            string[] dtmis = expectedDeps.Split(",", StringSplitOptions.RemoveEmptyEntries);
            string[] paths = expectedPaths.Split(",", StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(ReturnCodes.Success, returnCode);
            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));

            for (int i = 0; i < dtmis.Length; i++)
            {
                Assert.True(standardOut.Contains($"* Importing model \"{dtmis[i]}\"."));
                FileInfo modelFile = new FileInfo(Path.GetFullPath(testImportRepo.FullName + "/" + paths[i]));
                Assert.True(modelFile.Exists);
                Assert.AreEqual(dtmis[i], ParsingUtils.GetRootId(modelFile));
            }
        }

        [TestCase("dtmi/com/example/invalidmodel-2.json")]
        public void ImportModelFileErrorInvalidDTDL(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"import --model-file \"{qualifiedModelFilePath}\" {targetRepo}");

            Assert.AreEqual(ReturnCodes.ValidationError, returnCode);

            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
        }

        [TestCase("dtmi/com/example/invalidmodel-1.json")]
        public void ImportModelFileErrorResolutionFailure(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"import --model-file \"{qualifiedModelFilePath}\" {targetRepo}");

            Assert.AreEqual(ReturnCodes.ResolutionError, returnCode);

            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
        }

        [TestCase("dtmi/strict/namespaceconflict-1.json", "dtmi:strict:namespaceconflict;1", "dtmi:com:example:acceleration;1")]
        public void ImportModelFileErrorStrictRuleIdNamespaceConformance(string modelFilePath, string rootDtmi, string violationDtmi)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"import --model-file \"{qualifiedModelFilePath}\" {targetRepo}");

            Assert.AreEqual(ReturnCodes.ValidationError, returnCode);

            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
            Assert.True(standardOut.Contains($"* Ensuring DTMIs namespace conformance for model \"{rootDtmi}\"."));
            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardError.Contains(violationDtmi));
        }

        [TestCase("dtmi/strict/nondtdl-1.json")]
        public void ImportModelFileErrorNonDtdlContent(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, _, string standardError) =
                ClientInvokator.Invoke($"import --model-file \"{qualifiedModelFilePath}\" {targetRepo}");

            Assert.AreEqual(ReturnCodes.InvalidArguments, returnCode);
            Assert.True(standardError.Contains($"{Outputs.DefaultErrorToken} Model file contents of json type 'String' is not supported."));
        }

        [TestCase("dtmi/strict/emptyarray-1.json")]
        public void ImportModelFileErrorEmptyJsonArray(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, _, string standardError) =
                ClientInvokator.Invoke($"import --model-file \"{qualifiedModelFilePath}\" {targetRepo}");

            Assert.AreEqual(ReturnCodes.InvalidArguments, returnCode);
            Assert.True(standardError.Contains($"{Outputs.DefaultErrorToken} No models to validate."));
        }

        [TestCase("dtmi/com/example/thermostat-1.json")]
        public void ImportModelFileSilentNoStandardOut(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"import --silent --model-file \"{qualifiedModelFilePath}\" {targetRepo}");

            Assert.AreEqual(ReturnCodes.Success, returnCode);
            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.AreEqual(string.Empty, standardOut);
        }

        [TestCase("dtmi/com/example/thermostat-1.json")]
        public void ImportSupportsDebugHeaders(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"import --silent --model-file \"{qualifiedModelFilePath}\" {targetRepo} --debug");

            Assert.AreEqual(ReturnCodes.Success, returnCode);
            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.AreEqual(string.Empty, standardOut);
            Assert.True(standardError.Contains(Outputs.DebugHeader));
        }

        [TestCase("ontology", null, ReturnCodes.Success)]
        [TestCase("indexable", "*.json", ReturnCodes.Success)]
        [TestCase("indexable", "*temperaturecontroller-1.*", ReturnCodes.Success)]
        [TestCase("dtmi", "*namespaceconflict-1.json", ReturnCodes.ValidationError)]
        [TestCase("dtmi", "*invalidformat.json", ReturnCodes.InvalidArguments)]
        public void ImportModelsDirectory(string directory, string pattern, int expectedReturnCode)
        {
            string targetDirectoryPath = Path.Combine(TestHelpers.TestLocalModelRepository, directory);
            string searchPattern = string.IsNullOrEmpty(pattern) ? "" : $"--search-pattern {pattern}";
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"import --directory \"{targetDirectoryPath}\" {searchPattern} {targetRepo}");

            Assert.AreEqual(expectedReturnCode, returnCode, standardError);
            if (expectedReturnCode == ReturnCodes.Success)
            {
                Assert.False(standardError.Contains(Outputs.DefaultErrorToken), "Unexpected error token in stderr.");

                if (string.IsNullOrEmpty(pattern))
                {
                    pattern = "*.json";
                }

                var expectedImportedFiles = new List<FileInfo>();
                foreach (string file in Directory.EnumerateFiles(targetDirectoryPath, pattern,
                    new EnumerationOptions { RecurseSubdirectories = true }))
                {
                    expectedImportedFiles.Add(new FileInfo(file));
                }

                foreach (FileInfo expectedFile in expectedImportedFiles)
                {
                    string rootId = ParsingUtils.GetRootId(expectedFile);
                    string createPath = DtmiConventions.GetModelUri(rootId, new Uri(testImportRepo.FullName)).LocalPath;
                    Assert.True(new FileInfo(createPath).Exists, $"Expected model file import: '{createPath}' does not exist.");
                    Assert.True(standardOut.Contains($"[Validating]: {expectedFile.FullName}"));
                    Assert.True(standardOut.Contains($"* Ensuring DTMIs namespace conformance for model \"{rootId}\""));
                    Assert.True(standardOut.Contains($"* Importing model \"{rootId}\""));
                }
            }
        }

        [Test]
        public void ImportErrorsWithNoInput()
        {
            (int returnCode, _, string standardError) = ClientInvokator.Invoke($"import");

            Assert.AreEqual(ReturnCodes.InvalidArguments, returnCode);
            Assert.True(standardError.Contains("[Error]: Nothing to import!"), "Missing expected error message.");
        }

        [TestCase("dtmi/version3/emptyv3-1.json")]
        public void Importv3FailsIfNotSet(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"import --model-file \"{qualifiedModelFilePath}\" {targetRepo}");

            Assert.AreEqual(ReturnCodes.ValidationError, returnCode);

            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
        }

        [TestCase("dtmi/version3/emptyv3-1.json")]
        public void Importv3Ok(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string targetRepo = $"--local-repo \"{testImportRepo.FullName}\"";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"import --model-file \"{qualifiedModelFilePath}\" {targetRepo} --maxDtdlVersion 3");

            Assert.AreEqual(ReturnCodes.ValidationError, returnCode);

            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
        }
    }
}
