using NUnit.Framework;
using System.IO;

namespace Azure.IoT.DeviceModelsRepository.CLI.Tests
{
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

            Assert.False(standardError.Contains("ERROR:"));
            Assert.True(standardError.Contains(Outputs.StandardHeader));
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
            Assert.False(standardError.Contains("ERROR:"));
            Assert.True(standardOut.Contains("- Validating models conform to DTDL..."));

            if (strict)
            {
                Assert.True(standardOut.Contains("- Ensuring DTMIs namespace conformance for model"));
                Assert.True(standardOut.Contains("- Ensuring model file path adheres to DMR path conventions..."));
            }
        }

        [TestCase("dtmi/com/example/temperaturecontroller-1.expanded.json", false)]
        [TestCase("dtmi/com/example/temperaturecontroller-1.expanded.json", true)]
        public void ValidateModelFileArrayOfModelObjects(string modelFilePath, bool strict)
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
                Assert.False(standardError.Contains("ERROR:"));
                Assert.True(standardOut.Contains("- Validating models conform to DTDL..."));
                return;
            }

            // TODO: --strict validation is not fleshed out for an array of models.
            Assert.True(standardError.Contains("ERROR: Strict validation requires a single root model object."));
            Assert.AreEqual(Handlers.ReturnCodes.ValidationError, returnCode);
        }

        [TestCase("dtmi/com/example/invalidmodel-2.json")]
        public void ValidateModelFileErrorInvalidDTDL(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, string standardOut, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\" ");

            Assert.AreEqual(Handlers.ReturnCodes.ParserError, returnCode);

            Assert.True(standardError.Contains("ERROR:"));
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

            Assert.True(standardError.Contains("ERROR:"));
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
            Assert.True(standardError.Contains($"ERROR: "));
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
            Assert.True(standardError.Contains("ERROR: "));
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
            Assert.True(standardError.Contains("ERROR: Model file path validation requires a local repository."));
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
            Assert.True(standardError.Contains("ERROR: Importing model file contents of kind String is not yet supported."));
        }

        [TestCase("dtmi/strict/emptyarray-1.json")]
        public void ValidateModelFileErrorEmptyJsonArray(string modelFilePath)
        {
            string qualifiedModelFilePath = Path.Combine(TestHelpers.TestLocalModelRepository, modelFilePath);

            (int returnCode, _, string standardError) =
                ClientInvokator.Invoke($"" +
                $"validate --model-file \"{qualifiedModelFilePath}\" " +
                $"--repo \"{TestHelpers.TestLocalModelRepository}\"");

            Assert.AreEqual(Handlers.ReturnCodes.ValidationError, returnCode);
            Assert.True(standardError.Contains("ERROR: No models to validate."));
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
            Assert.True(!standardError.Contains("ERROR:"));
            Assert.AreEqual(string.Empty, standardOut);
        }
    }
}
