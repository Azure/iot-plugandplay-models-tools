using Azure.DigitalTwins.Validator.Exceptions;
using NUnit.Framework;

namespace Azure.DigitalTwins.Validator.Tests
{
    public class ScanForReservedWordsTests
    {
        [Test]
        public void should_pass_if_root_id_contains_no_reserved_words()
        {
            Validations.ScanForReservedWords("{\"@context\": \"dtmi:dtdl:context;2\", " +
                "\"@id\": \"dtmi:com:example:device;1\", " +
                "\"@type\": \"Interface\", " +
                "\"displayName\": \"Azure Device\", " +
                "\"contents\": [{ \"@type\": \"Property\", " +
                "  \"name\": \"Failure\", " +
                "  \"schema\": \"boolean\" } ]}");

        }

        [Test]
        public void should_pass_if_sub_id_contains_no_reserved_words()
        {
                Validations.ScanForReservedWords("{\"@context\": \"dtmi:dtdl:context;2\", " +
                        "\"@id\": \"dtmi:com:example:device;1\", " +
                        "\"@type\": \"Interface\", " +
                        "\"displayName\": \"Azure Device\", " +
                        "\"contents\": [{\"@type\": \"Property\", " +
                        "  \"@id\": \"dtmi:com:example:device:property;1\", " +
                        "  \"name\": \"Failure\", " +
                        "  \"schema\": \"boolean\" }]}");
        }

        [Test]
        public void should_fail_if_root_id_contains_Azure()
        {
            Assert.Throws<ReservedWordException>(
                () => Validations.ScanForReservedWords("{\"@context\": \"dtmi:dtdl:context;2\", " +
                "\"@id\": \"dtmi:com:AzUrE:device;1\", " +
                "\"@type\": \"Interface\", " +
                "\"displayName\": \"Azure Device\", " +
                "\"contents\": [{\"@type\": \"Property\", " +
                "  \"name\": \"Failure\", " +
                "  \"schema\": \"boolean\"}]}"));
        }

        [Test]
        public void should_fail_if_sub_id_contains_Azure()
        {
            Assert.Throws<ReservedWordException>(
                () => Validations.ScanForReservedWords("{\"@context\": \"dtmi:dtdl:context;2\", " +
                "\"@id\": \"dtmi:com:test:device;1\", " +
                "\"@type\": \"Interface\", " +
                "\"displayName\": \"Azure Device\", " +
                "\"contents\": [{\"@type\": \"Property\", " +
                "  \"@id\": \"dtmi:com:test:device:Azure;1\", " +
                "  \"name\": \"Failure\", " +
                "  \"schema\": \"boolean\" }]}"));
        }

        [Test]
        public void should_fail_if_root_id_contains_Microsoft()
        {
            Assert.Throws<ReservedWordException>(
                () => Validations.ScanForReservedWords("{\"@context\": \"dtmi:dtdl:context;2\", " +
                "\"@id\": \"dtmi:com:MicroSoft:device;1\", " +
                "\"@type\": \"Interface\", " +
                "\"displayName\": \"Microsoft Device\", " +
                "\"contents\": [{\"@type\": \"Property\"," +
                "  \"name\": \"Failure\", " +
                "  \"schema\": \"boolean\"}]}"));
        }

        [Test]
        public void should_fail_if_sub_id_contains_Microsoft()
        {
            Assert.Throws<ReservedWordException>(
                () => Validations.ScanForReservedWords("{\"@context\": \"dtmi:dtdl:context;2\"," +
                        "\"@id\": \"dtmi:com:test:device;1\"," +
                        "\"@type\": \"Interface\", " +
                        "\"displayName\": \"Microsoft Device\", " +
                        "\"contents\": [{\"@type\": \"Property\", " +
                        "  \"@id\": \"dtmi:com:test:device:microsoft;1\", " +
                        "  \"name\": \"Failure\", " +
                        " \"schema\": \"boolean\"}]}"));
        }

    }
}
