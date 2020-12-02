using System;
using System.Runtime.Serialization;

namespace StateChartsDotNet.Common.Exceptions
{
    public sealed class MetadataValidationException : StateChartException
    {
        internal MetadataValidationException(string message)
            : base(message)
        {
        }

        internal MetadataValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal MetadataValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
