namespace Azure.DigitalTwins.Validator.Exceptions
{
    public class InvalidContextException : ValidationException
    {
        public InvalidContextException(string fileName) :
        base($"File '{fileName}' has an invalid \"@context\" element")
        { }
    }
}