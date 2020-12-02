using System;
using System.Runtime.Serialization;

namespace StateChartsDotNet.Common.Exceptions
{
    public sealed class CommunicationException : StateChartException
    {
        internal CommunicationException(string message)
            : base(message)
        {
        }

        internal CommunicationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal CommunicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
