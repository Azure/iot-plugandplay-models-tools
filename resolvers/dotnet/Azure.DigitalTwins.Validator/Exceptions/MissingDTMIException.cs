using System;
using System.Diagnostics;
using System.IO;

namespace Azure.DigitalTwins.Validator.Exceptions
{
    public class MissingDTMIException : ValidationException
    {
        public MissingDTMIException(string fileName) :
        base($"File '{fileName}' does not have a root \"@id\" element")
        { }
    }
}