using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Azure.DigitalTwins.Validator.Exceptions
{
    public class ReservedWordException : ValidationException
    {
        public ReservedWordException(IEnumerable<string> reservedWords) : base($"Reserved words found in the following:\n{string.Join(",\n", reservedWords)}")
        {
        }
    }
}