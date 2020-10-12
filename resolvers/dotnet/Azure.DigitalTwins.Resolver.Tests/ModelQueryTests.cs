using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver.Tests
{
    public class ModelQueryTests
    {
        readonly string _modelTemplate = @"{{
            {0}
            ""@type"": ""Interface"",
            ""displayName"": ""Phone"",
            {1}
            {2}
            ""@context"": ""dtmi:dtdl:context;2""
        }}";

        [TestCase("\"@id\": \"dtmi:com:example:thermostat;1\",", "dtmi:com:example:thermostat;1")]
        [TestCase("\"@id\": \"\",", "")]
        [TestCase("", "")]
        public void GetId(string formatId, string expectedId)
        {
            string modelContent = string.Format(_modelTemplate, formatId, "", "");
            ModelQuery query = new ModelQuery(modelContent);
            Assert.AreEqual(query.GetId(), expectedId);
        }

        [TestCase(
            @"
            ""contents"":
            [{
                ""@type"": ""Property"",
                ""name"": ""capacity"",
                ""schema"": ""integer""
            },
            {
                ""@type"": ""Component"",
                ""name"": ""frontCamera"",
                ""schema"": ""dtmi:com:example:Camera;3""
            },
            {
                ""@type"": ""Component"",
                ""name"": ""backCamera"",
                ""schema"": ""dtmi:com:example:Camera;3""
            },
            {
                ""@type"": ""Component"",
                ""name"": ""deviceInfo"",
                ""schema"": ""dtmi:azure:DeviceManagement:DeviceInformation;1""
            }],",
            "dtmi:com:example:Camera;3,dtmi:com:example:Camera;3,dtmi:azure:DeviceManagement:DeviceInformation;1")]
        [TestCase(
            @"
            ""contents"":
            [{
              ""@type"": ""Property"",
              ""name"": ""capacity"",
              ""schema"": ""integer""
            }],", "")]
        [TestCase(@"""contents"":[],", "")]
        [TestCase("", "")]
        public void GetComponentSchema(string contents, string expected)
        {
            string[] expectedDtmis = expected.Split(",", System.StringSplitOptions.RemoveEmptyEntries);
            string modelContent = string.Format(_modelTemplate, "", "", contents);
            ModelQuery query = new ModelQuery(modelContent);
            IList<string> componentSchemas = query.GetComponentSchemas();
            Assert.AreEqual(componentSchemas.Count, expectedDtmis.Length);

            foreach(string schema in componentSchemas)
            {
                Assert.Contains(schema, expectedDtmis);
            }
        }

        [TestCase(
            "\"extends\": [\"dtmi:com:example:Camera;3\",\"dtmi:azure:DeviceManagement:DeviceInformation;1\"],",
            "dtmi:com:example:Camera;3,dtmi:azure:DeviceManagement:DeviceInformation;1")]
        [TestCase("\"extends\": [],", "")]
        [TestCase("\"extends\": \"dtmi:com:example:Camera;3\",", "dtmi:com:example:Camera;3")]
        [TestCase("", "")]
        public void GetExtends(string extends, string expected)
        {
            string[] expectedDtmis = expected.Split(",", System.StringSplitOptions.RemoveEmptyEntries);
            string modelContent = string.Format(_modelTemplate, "", extends, "");
            ModelQuery query = new ModelQuery(modelContent);
            IList<string> extendsDtmis = query.GetExtends();
            Assert.AreEqual(extendsDtmis.Count, expectedDtmis.Length);

            foreach (string dtmi in extendsDtmis)
            {
                Assert.Contains(dtmi, expectedDtmis);
            }
        }

        [TestCase(
            "\"@id\": \"dtmi:com:example:thermostat;1\",",
            "\"extends\": [\"dtmi:com:example:Camera;3\",\"dtmi:azure:DeviceManagement:DeviceInformation;1\"],",
            @"""contents"":
            [{
              ""@type"": ""Property"",
              ""name"": ""capacity"",
              ""schema"": ""integer""
            },
            {
                ""@type"": ""Component"",
                ""name"": ""frontCamera"",
                ""schema"": ""dtmi:com:example:Camera;3""
            },
            {
                ""@type"": ""Component"",
                ""name"": ""backCamera"",
                ""schema"": ""dtmi:com:example:Camera;3""
            }],",
            "dtmi:com:example:Camera;3,dtmi:azure:DeviceManagement:DeviceInformation;1"
        )]
        public void GetModelDependencies(string id, string extends, string contents, string expected)
        {
            string[] expectedDtmis = expected.Split(",", System.StringSplitOptions.RemoveEmptyEntries);
            string modelContent = string.Format(_modelTemplate, id, extends, contents);
            ModelQuery.ModelMetadata metadata = new ModelQuery(modelContent).GetMetadata();

            IList<string> dependencies = metadata.Dependencies;

            Assert.AreEqual(dependencies.Count, expectedDtmis.Length);

            foreach (string dtmi in dependencies)
            {
                Assert.Contains(dtmi, expectedDtmis);
            }
        }

        [Test]
        public async Task ListToDictAsync()
        {
            string testRepoPath = TestHelpers.GetTestLocalModelRepository();
            string expandedContent = await File.ReadAllTextAsync(
                $"{testRepoPath}/dtmi/com/example/temperaturecontroller-1.expanded.json", Encoding.UTF8);
            ModelQuery query = new ModelQuery(expandedContent);
            Dictionary<string, string> transformResult = await query.ListToDictAsync();

            // Assert KPI's for TemperatureController;1.
            // Ensure transform of expanded content to dictionary is what we'd expect.
            string[] expectedIds = new string[] { 
                "dtmi:azure:DeviceManagement:DeviceInformation;1",
                "dtmi:com:example:Thermostat;1",
                "dtmi:com:example:TemperatureController;1" };

            Assert.True(transformResult.Keys.Count == expectedIds.Length);

            foreach (string id in expectedIds)
            {
                Assert.True(transformResult.ContainsKey(id));
                Assert.True(TestHelpers.ParseRootDtmiFromJson(transformResult[id]).Equals(id, System.StringComparison.Ordinal));
            }
        }
    }
}
