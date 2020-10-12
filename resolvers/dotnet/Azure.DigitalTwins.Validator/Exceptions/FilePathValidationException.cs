using System;
using System.IO;

namespace Azure.DigitalTwins.Validator.Exceptions
{
    public class FilePathValidationException : ValidationException
    {
        private static string GenericError(FileInfo fileInfo)
        {
            return $"File '{fileInfo.FullName}' does not adhere to naming rules.";
        }
        public FilePathValidationException(FileInfo fileInfo):
        base(GenericError(fileInfo))
        {}
        public FilePathValidationException(FileInfo fileInfo, string message) :
            base($"{GenericError(fileInfo)}{message}")
        {
        }

        public FilePathValidationException(FileInfo fileInfo, string message, Exception innerException) :
            base($"{GenericError(fileInfo)}{message}", innerException)
        {
        }
    }
}