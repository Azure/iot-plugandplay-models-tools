using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Azure.DigitalTwins.Validator.Exceptions {
    public class InvalidSubDTMIException : ValidationException
    {
        public InvalidSubDTMIException(IEnumerable<string> subDTMI): base($"Invalid sub DTMI format in the following:\n${string.Join(",\n", subDTMI)}")
        {
        }
                public InvalidSubDTMIException(string subDTMI): base($"Invalid sub DTMI format:\n${subDTMI}")
        {
        }
    }
}