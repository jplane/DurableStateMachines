using System;
using System.Runtime.Serialization;

namespace StateChartsDotNet.Common.Exceptions
{
    public class StateChartException : ApplicationException
    {
        internal StateChartException(string message)
            : base(message)
        {
        }

        internal StateChartException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal StateChartException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
