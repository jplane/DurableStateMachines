using System;
using System.Runtime.Serialization;

namespace StateChartsDotNet.CoreEngine.Abstractions.Model
{
    public class ModelValidationException : ApplicationException
    {
        public ModelValidationException(string message)
            : base(message)
        {
        }

        public ModelValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ModelValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
