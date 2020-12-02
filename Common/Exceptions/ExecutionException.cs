using System;
using System.Runtime.Serialization;

namespace StateChartsDotNet.Common.Exceptions
{
    public sealed class ExecutionException : StateChartException
    {
        internal ExecutionException(string message)
            : base(message)
        {
        }

        internal ExecutionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal ExecutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
