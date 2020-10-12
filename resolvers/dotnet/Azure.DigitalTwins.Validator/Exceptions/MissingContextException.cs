namespace Azure.DigitalTwins.Validator.Exceptions
{
    public class MissingContextException: ValidationException
    {
        public MissingContextException(string fileName):
        base($"File '{fileName}' does not have a root \"@context\" element")
        {}
    }
}