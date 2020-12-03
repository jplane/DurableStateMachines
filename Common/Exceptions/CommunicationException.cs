using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace StateChartsDotNet.Common.Exceptions
{
    public sealed class CommunicationException : StateChartException
    {
        public CommunicationException()
        {
        }

        public CommunicationException(string message)
            : base(message)
        {
        }

        public CommunicationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
