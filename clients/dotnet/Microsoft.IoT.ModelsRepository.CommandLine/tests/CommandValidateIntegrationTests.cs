// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Microsoft.IoT.ModelsRepository.CommandLine.Tests
{
    [NonParallelizable]
    public class CommandValidateIntegrationTests
    {
        [TestCase("dtmi/com/example/thermostat-1.json", false)]
        [TestCase("dtmi/com/example/thermostat-1.json", true)]
        public void ValidateModelFileSingleModelObject(string modelFilePath, bool strict)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string strictSwitch = strict ? "--strict" : "";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" " +
                $"{strictSwitch}");
            Assert.AreEqual(ReturnCodes.Success, returnCode);

            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));

            if (strict)
            {
                Assert.True(standardOut.Contains("* Ensuring DTMIs namespace conformance for model"));
                Assert.True(standardOut.Contains("* Ensuring model file path adheres to DMR path conventions."));
            }
        }

        [TestCase("dtmi/com/example/temperaturecontroller-1.json", true, TestHelpers.ClientType.Local)]
        [TestCase("dtmi/com/example/temperaturecontroller-1.json", false, TestHelpers.ClientType.Remote)]
        [TestCase("dtmi/com/example/temperaturecontroller-1.json", true, TestHelpers.ClientType.Local)]
        public void ValidateModelFileSingleModelObjectWithDeps(string modelFilePath, bool strict, TestHelpers.ClientType clientType)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string strictSwitch = strict ? "--strict" : "";
            string targetRepo = string.Empty;
            if (clientType == TestHelpers.ClientType.Local)
            {
                targetRepo = $"--repo \"{TestHelpers.TestLocalModelRepository}\"";
            }

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"validate --model-file \"{qualifiedModelFilePath}\" {targetRepo} {strictSwitch}");

            Assert.AreEqual(ReturnCodes.Success, returnCode);
            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));

            if (strict)
            {
                Assert.True(standardOut.Contains("* Ensuring DTMIs namespace conformance for model"));
                Assert.True(standardOut.Contains("* Ensuring model file path adheres to DMR path conventions."));
            }
        }

        [TestCase("dtmi/com/example/temperaturecontroller-1.expanded.json", false)]
        [TestCase("dtmi/com/example/temperaturecontroller-1.expanded.json", true)]
        public void ValidateModelFileArrayOfModels(string modelFilePath, bool strict)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);
            string strictSwitch = strict ? "--strict" : "";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" " +
                $"{strictSwitch}");

            if (!strict)
            {
                Assert.AreEqual(ReturnCodes.Success, returnCode);
                Assert.False(standardError.Contains(Outputs.DefaultErrorToken));
                Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
                return;
            }

            // TODO: --strict validation is not fleshed out for an array of models.
            Assert.True(standardError.Contains($"{Outputs.DefaultErrorToken} Strict validation requires a single root model object."));
            Assert.AreEqual(ReturnCodes.ValidationError, returnCode);
        }

        [TestCase("dtmi/com/example/incompleteexpanded-1.expanded.json", "dtmi:azure:DeviceManagement:DeviceInformation;1")]
        [TestCase("dtmi/com/example/incompleteexpanded-2.expanded.json", "dtmi:com:example:Thermostat;1 dtmi:azure:DeviceManagement:DeviceInformation;1")]
        public void ValidateModelFileErrorIncompleteArrayOfModels(string modelFilePath, string missingReferences)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string _, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" ");

            Assert.True(standardError.Contains(
                $"{Outputs.DefaultErrorToken} No DtmiResolverAsync provided to resolve requisite reference(s): "));
                //+missingReferences));
            Assert.AreEqual(ReturnCodes.ResolutionError, returnCode);
        }

        [TestCase("dtmi/com/example/invalidmodel-2.json")]
        public void ValidateModelFileErrorInvalidDTDL(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" ");

            Assert.AreEqual(ReturnCodes.ValidationError, returnCode);

            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
        }

        [TestCase("dtmi/com/example/invalidmodel-1.json")]
        public void ValidateModelFileErrorResolutionFailure(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" ");

            Assert.AreEqual(ReturnCodes.ResolutionError, returnCode);

            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
        }

        [TestCase("dtmi/strict/namespaceconflict-1.json", "dtmi:strict:namespaceconflict;1", "dtmi:com:example:acceleration;1")]
        public void ValidateModelFileErrorStrictRuleIdNamespaceConformance(string modelFilePath, string rootDtmi, string violationDtmi)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" --strict");

            Assert.AreEqual(ReturnCodes.ValidationError, returnCode);

            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
            Assert.True(standardOut.Contains($"* Ensuring DTMIs namespace conformance for model \"{rootDtmi}\"."));
            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardError.Contains(violationDtmi));
        }

        [TestCase("dtmi/strict/badfilepath-1.json", "dtmi:com:example:Freezer;1")]
        public void ValidateModelFileErrorStrictRuleIdFilePath(string modelFilePath, string rootDtmi)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" --strict");

            Assert.AreEqual(ReturnCodes.ValidationError, returnCode);

            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
            Assert.True(standardOut.Contains($"* Ensuring DTMIs namespace conformance for model \"{rootDtmi}\"."));
            Assert.True(standardOut.Contains($"* Ensuring model file path adheres to DMR path conventions."));
            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardError.Contains($"File \"{Path.GetFullPath(qualifiedModelFilePath)}\" does not adhere to DMR path conventions. "));
        }

        [TestCase("dtmi/com/example/thermostat-1.json")]
        public void ValidateModelFileErrorStrictRuleFilePathRequiresLocalRepo(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, _, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"http://localhost\" --strict");

            Assert.AreEqual(ReturnCodes.ValidationError, returnCode);
            Assert.True(standardError.Contains($"{Outputs.DefaultErrorToken} Model file path validation requires a local repository."));
        }

        [TestCase("dtmi/strict/nondtdl-1.json")]
        public void ValidateModelFileErrorNonDtdlContent(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, _, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\"");

            Assert.AreEqual(ReturnCodes.InvalidArguments, returnCode);
            Assert.True(standardError.Contains($"{Outputs.DefaultErrorToken} Model file contents of json type 'String' is not supported."));
        }

        [TestCase("dtmi/strict/emptyarray-1.json")]
        public void ValidateModelFileErrorEmptyJsonArray(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, _, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\"");

            Assert.AreEqual(ReturnCodes.InvalidArguments, returnCode);
            Assert.True(standardError.Contains($"{Outputs.DefaultErrorToken} No models to validate."));
        }

        [TestCase("dtmi/com/example/thermostat-1.json")]
        public void ValidateModelFileSilentNoStandardOut(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --silent --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\"");

            Assert.AreEqual(ReturnCodes.Success, returnCode);
            Assert.True(!standardError.Contains(Outputs.DefaultErrorToken));
            Assert.AreEqual(string.Empty, standardOut);
        }

        [TestCase("dtmi/com/example/thermostat-1.json")]
        public void ValidateModelSupportsDebugHeaders(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --silent --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" --debug");

            Assert.AreEqual(ReturnCodes.Success, returnCode);
            Assert.True(!standardError.Contains(Outputs.DefaultErrorToken));
            Assert.AreEqual(string.Empty, standardOut);
            Assert.True(standardError.Contains(Outputs.DebugHeader));
        }

        [TestCase("dtmi/centralctx/locationpoint-1.json", ReturnCodes.ValidationError)]
        [TestCase("dtmi/centralctx/locationpoint-2.json", ReturnCodes.Success)]
        public void ValidateModelUsingCentralContext(string modelFilePath, int expectedReturnCode)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" ");

            Assert.AreEqual(expectedReturnCode, returnCode);

            if (expectedReturnCode == ReturnCodes.ValidationError)
            {
                Assert.True(standardError.Contains("has value 'geopoint' that is neither a valid DTMI reference nor a DTDL term"));
                return;
            }
            
            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
        }

        [TestCase("indexable", "deviceinformation*", "deviceinformation-1.json", false, ReturnCodes.Success)]
        [TestCase("indexable", "deviceinformation*", "deviceinformation-1.json", true, ReturnCodes.Success)]
        [TestCase("indexable", "*xtag_*", "xtag_ble_acc-1.json,xtag_usb_acc-1.json", false, ReturnCodes.Success)]
        [TestCase("indexable", "*xtag_*", "xtag_ble_acc-1.json,xtag_usb_acc-1.json", true, ReturnCodes.Success)]
        [TestCase("indexable", null, null, false, ReturnCodes.Success)]
        [TestCase("indexable", null, null, true, ReturnCodes.Success)]
        [TestCase("dtmi", "badfilepath-1*", null, true, ReturnCodes.ValidationError)]
        [TestCase("dtmi", "temperaturecontroller-1.expanded.json", null, true, ReturnCodes.ValidationError)]
        [TestCase("dtmi", "nondtdl*", null, false, ReturnCodes.InvalidArguments)]
        public void ValidateModelsDirectory(string directory, string pattern, string expectedFiles, bool isStrict, int expectedReturnCode)
        {
            string targetDirectoryPath = Path.Combine(TestHelpers.TestLocalModelRepository, directory);
            string searchPattern = string.IsNullOrEmpty(pattern) ? "" : $"--search-pattern {pattern}";
            string resolutionRepo = isStrict ? $"--repo {Path.Combine(TestHelpers.TestLocalModelRepository, "indexable")}" : "";
            string strict = isStrict ? "--strict" : "";

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"validate --directory \"{targetDirectoryPath}\" {searchPattern} {resolutionRepo} {strict}");

            Assert.AreEqual(expectedReturnCode, returnCode);

            if (expectedReturnCode == ReturnCodes.Success)
            {
                Assert.False(standardError.Contains(Outputs.DefaultErrorToken), "Unexpected error token in stderr.");
            }

            if (!string.IsNullOrEmpty(expectedFiles))
            {
                string[] enumerateFiles = expectedFiles.Split(",");
                foreach (string file in enumerateFiles)
                {
                    Assert.True(standardOut.Contains(file));
                }

                string[] validations = standardOut.Split("[Validating]:", System.StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(enumerateFiles.Length, validations.Length);
            }
        }

        [Test]
        public void ValidateModelErrorsWithNoInput()
        {
            (int returnCode, _, string standardError) = ClientInvokator.Invoke($"validate");

            Assert.AreEqual(ReturnCodes.InvalidArguments, returnCode);
            Assert.True(standardError.Contains("[Error]: Nothing to validate!"), "Missing expected error message.");
        }

        [TestCase("dtmi/version3/emptyv3-1.json", ReturnCodes.ValidationError)]
        [TestCase("dtmi/centralctx/locationpoint-2.json", ReturnCodes.Success)]
        public void ValidateModelWithMaxDtdlSetTo2(string modelFilePath, int expectedReturnCode)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--maxDtdlVersion 2 " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" ");

            Assert.AreEqual(expectedReturnCode, returnCode);

            if (expectedReturnCode == ReturnCodes.ValidationError)
            {
                Assert.True(standardError.Contains("@context specifier has value 'dtmi:dtdl:context;3', which specifies a DTDL version that exceeds the configured max version of 2"));
                return;
            }

            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
        }

        [TestCase("dtmi/version3/emptyv3-1.json", ReturnCodes.Success)]
        [TestCase("dtmi/centralctx/locationpoint-2.json", ReturnCodes.Success)]
        public void ValidateModelWithMaxDtdlSetTo3(string modelFilePath, int expectedReturnCode)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--maxDtdlVersion 3 " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" ");

            Assert.AreEqual(expectedReturnCode, returnCode);
            Assert.True(standardOut.Contains("* Validating model file content conforms to DTDL."));
        }
    }
}
