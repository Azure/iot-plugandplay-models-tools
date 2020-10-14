using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Azure.DigitalTwins.Validator.Exceptions
{
    public class InvalidDTMIException : ValidationException
    {
        public InvalidDTMIException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public InvalidDTMIException(IEnumerable<string> dtmi) : base($"Invalid DTMI format in the following:\n{string.Join(",\n", dtmi)}")
        {
        }
        public InvalidDTMIException(string dtmi) : base($"Invalid DTMI format:\n{dtmi}")
        {
        }
    }
}