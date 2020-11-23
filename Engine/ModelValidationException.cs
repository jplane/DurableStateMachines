using System;
using System.Runtime.Serialization;

namespace StateChartsDotNet
{
    public class ModelValidationException : ApplicationException
    {
        internal ModelValidationException(string message)
            : base(message)
        {
        }

        internal ModelValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ModelValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
