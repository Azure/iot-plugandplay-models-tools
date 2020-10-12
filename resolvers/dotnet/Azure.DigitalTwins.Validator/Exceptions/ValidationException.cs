using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Azure.DigitalTwins.Validator.Exceptions
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public abstract class ValidationException: Exception {
        protected ValidationException()
        {
        }

        protected ValidationException(string message) : base(message)
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