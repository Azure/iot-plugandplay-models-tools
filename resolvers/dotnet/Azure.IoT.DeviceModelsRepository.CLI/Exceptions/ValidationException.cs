using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Azure.IoT.DeviceModelsRepository.CLI.Exceptions
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class ValidationException : Exception
    {
        protected ValidationException()
        {
        }

        public ValidationException(string message) : base(message)
        {
        }

        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private string GetDebuggerDisplay()
        {
            return this.Message;
        }
    }
}
