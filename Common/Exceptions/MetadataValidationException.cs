using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace StateChartsDotNet.Common.Exceptions
{
    public sealed class MetadataValidationException : StateChartException
    {
        public MetadataValidationException()
        {
        }

        public MetadataValidationException(string message)
            : base(message)
        {
        }

        public MetadataValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
