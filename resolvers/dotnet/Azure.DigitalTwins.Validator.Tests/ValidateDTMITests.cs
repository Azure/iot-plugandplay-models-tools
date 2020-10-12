using NUnit.Framework;

namespace Azure.DigitalTwins.Validator.Tests
{
    public class ValidateDTMITests
    {
        [Test]
        public void should_pass_valid_DTMI() {
/*{"@id": "dtmi:com:example:ThermoStat;1"}*/
        }

        [Test]
        public void should_fail_invalid_DTMI_missing_semicolon() {
/*{"@id": "dtmi:com:example:ThermoStat-1"}*/
        }

        [Test]
        public void should_fail_invalid_DTMI_missing_dtmi() {
/*{"@id": "com:example:ThermoStat;1"}*/
        }

        [Test]
        public void should_pass_valid_sub_DTMI() {
/*{
            "@context": "dtmi:dtdl:context;2",
            "@id": "dtmi:com:test:device;1",
            "@type": "Interface",
            "displayName": "Microsoft Device",
            "contents": [
                {
                    "@type": "Property",
                    "@id": "dtmi:com:test:device:property;1",
                    "name": "Failure",
                    "schema": "boolean"
                }
            ]
        }*/
        }

        [Test]
        public void should_fail_invalid_sub_DTMI_not_namespaced() {
/*{
            "@context": "dtmi:dtdl:context;2",
            "@id": "dtmi:com:test:device;1",
            "@type": "Interface",
            "displayName": "Microsoft Device",
            "contents": [
                {
                    "@type": "Property",
                    "@id": "dtmi:com:otherScope:property;1",
                    "name": "Failure",
                    "schema": "boolean"
                }
            ]
        }*/
        }

        [Test]
        public void should_fail_invalid_sub_DTMI_missing_semicolon() {
/*{
            "@context": "dtmi:dtdl:context;2",
            "@id": "dtmi:com:test:device;1",
            "@type": "Interface",
            "displayName": "Microsoft Device",
            "contents": [
                {
                    "@type": "Property",
                    "@id": "com:test:device:property;1",
                    "name": "Failure",
                    "schema": "boolean"
                }
            ]
        }*/
        }
    }
}
