using System;
using System.Diagnostics;
using System.IO;

namespace Azure.DigitalTwins.Validator.Exceptions{
    public class MissingDTMIException: ValidationException
    {
        public MissingDTMIException(FileInfo fileInfo):
        base($"File '${fileInfo.FullName}' does not have a root \"@id\" element")
        {}
    }
}