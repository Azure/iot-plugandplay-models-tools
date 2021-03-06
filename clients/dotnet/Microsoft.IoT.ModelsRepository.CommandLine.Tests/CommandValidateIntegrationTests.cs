﻿using NUnit.Framework;
using System.IO;

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
            Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);

            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("- Validating models conform to DTDL..."));

            if (strict)
            {
                Assert.True(standardOut.Contains("- Ensuring DTMIs namespace conformance for model"));
                Assert.True(standardOut.Contains("- Ensuring model file path adheres to DMR path conventions..."));
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

            Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);
            Assert.False(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("- Validating models conform to DTDL..."));

            if (strict)
            {
                Assert.True(standardOut.Contains("- Ensuring DTMIs namespace conformance for model"));
                Assert.True(standardOut.Contains("- Ensuring model file path adheres to DMR path conventions..."));
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
                Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);
                Assert.False(standardError.Contains(Outputs.DefaultErrorToken));
                Assert.True(standardOut.Contains("- Validating models conform to DTDL..."));
                return;
            }

            // TODO: --strict validation is not fleshed out for an array of models.
            Assert.True(standardError.Contains($"{Outputs.DefaultErrorToken} Strict validation requires a single root model object."));
            Assert.AreEqual(Handlers.ReturnCodes.ValidationError, returnCode);
        }

        [TestCase("dtmi/com/example/incompleteexpanded-1.expanded.json")]
        public void ValidateModelFileErrorIncompleteArrayOfModels(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string _, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" ");

            Assert.True(standardError.Contains(
                $"{Outputs.DefaultErrorToken} DtmiResolver failed to resolve requisite references to element(s): " +
                "dtmi:azure:DeviceManagement:DeviceInformation;1"));
            Assert.AreEqual(Handlers.ReturnCodes.ResolutionError, returnCode);
        }

        [TestCase("dtmi/com/example/invalidmodel-2.json")]
        public void ValidateModelFileErrorInvalidDTDL(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" ");

            Assert.AreEqual(Handlers.ReturnCodes.ValidationError, returnCode);

            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("- Validating models conform to DTDL..."));
        }

        [TestCase("dtmi/com/example/invalidmodel-1.json")]
        public void ValidateModelFileErrorResolutionFailure(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" ");

            Assert.AreEqual(Handlers.ReturnCodes.ResolutionError, returnCode);

            Assert.True(standardError.Contains(Outputs.DefaultErrorToken));
            Assert.True(standardOut.Contains("- Validating models conform to DTDL..."));
        }

        [TestCase("dtmi/strict/namespaceconflict-1.json", "dtmi:strict:namespaceconflict;1", "dtmi:com:example:acceleration;1")]
        public void ValidateModelFileErrorStrictRuleIdNamespaceConformance(string modelFilePath, string rootDtmi, string violationDtmi)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" --strict");

            Assert.AreEqual(Handlers.ReturnCodes.ValidationError, returnCode);

            Assert.True(standardOut.Contains("- Validating models conform to DTDL..."));
            Assert.True(standardOut.Contains($"- Ensuring DTMIs namespace conformance for model \"{rootDtmi}\"..."));
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

            Assert.AreEqual(Handlers.ReturnCodes.ValidationError, returnCode);

            Assert.True(standardOut.Contains("- Validating models conform to DTDL..."));
            Assert.True(standardOut.Contains($"- Ensuring DTMIs namespace conformance for model \"{rootDtmi}\"..."));
            Assert.True(standardOut.Contains($"- Ensuring model file path adheres to DMR path conventions..."));
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

            Assert.AreEqual(Handlers.ReturnCodes.ValidationError, returnCode);
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

            Assert.AreEqual(Handlers.ReturnCodes.InvalidArguments, returnCode);
            Assert.True(standardError.Contains($"{Outputs.DefaultErrorToken} Importing model file contents of kind String is not yet supported."));
        }

        [TestCase("dtmi/strict/emptyarray-1.json")]
        public void ValidateModelFileErrorEmptyJsonArray(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, _, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\"");

            Assert.AreEqual(Handlers.ReturnCodes.InvalidArguments, returnCode);
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

            Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);
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

            Assert.AreEqual(Handlers.ReturnCodes.Success, returnCode);
            Assert.True(!standardError.Contains(Outputs.DefaultErrorToken));
            Assert.AreEqual(string.Empty, standardOut);
            Assert.True(standardError.Contains(Outputs.DebugHeader));
        }

        [TestCase("dtmi/centralctx/locationpoint-1.json", 2)]
        [TestCase("dtmi/centralctx/locationpoint-2.json", 0)]
        public void ValidateModelUsingCentralContext(string modelFilePath, int expectedReturnCode)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" ");

            Assert.AreEqual(expectedReturnCode, returnCode);

            if (expectedReturnCode == 2)
            {
                Assert.True(standardError.Contains("has value 'geopoint' that is not a DTMI or a DTDL term"));
                return;
            }
            
            Assert.True(standardOut.Contains("- Validating models conform to DTDL..."));
        }
    }
}
