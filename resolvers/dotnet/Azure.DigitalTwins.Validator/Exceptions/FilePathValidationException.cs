using System;
using System.IO;

namespace Azure.DigitalTwins.Validator.Exceptions
{
    public class FilePathValidationException : ValidationException
    {
        private static string GenericError(string filePath)
        {
            return $"File '{filePath}' does not adhere to naming rules.";
        }

        public FilePathValidationException(string filePath):
        base(GenericError(filePath))
        {}
        public FilePathValidationException(FileInfo fileInfo):
        base(GenericError(fileInfo.FullName))
        {}
        public FilePathValidationException(FileInfo fileInfo, string message) :
            base($"{GenericError(fileInfo.FullName)}{message}")
        {
        }

        public FilePathValidationException(FileInfo fileInfo, string message, Exception innerException) :
            base($"{GenericError(fileInfo.FullName)}{message}", innerException)
        {
        }
    }
}