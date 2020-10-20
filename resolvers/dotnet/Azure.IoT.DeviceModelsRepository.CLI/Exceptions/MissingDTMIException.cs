namespace Azure.IoT.DeviceModelsRepository.CLI.Exceptions
{
    public class MissingDTMIException : ValidationException
    {
        public MissingDTMIException(string fileName) :
        base($"File '{fileName}' does not have a root \"@id\" element")
        { }
    }
}
