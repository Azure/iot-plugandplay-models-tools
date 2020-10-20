using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Azure.IoT.DeviceModelsRepository.CLI.Exceptions
{
    public class InvalidDTMIException : ValidationException
    {
        public InvalidDTMIException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public InvalidDTMIException(IEnumerable<string> dtmi) : base($"Invalid DTMI format in the following:\n{string.Join(",\n", dtmi)}")
        {
        }
        public InvalidDTMIException(JsonElement dtmi) : base($"Invalid DTMI format:\n{dtmi.GetString()}")
        {
        }
        public InvalidDTMIException(string fileName) : base($"Invalid DTMI format in file:\n{fileName}")
        {
        }
    }
}
