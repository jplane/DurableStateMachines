using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace StateChartsDotNet.Common.Exceptions
{
    public sealed class ExecutionException : StateChartException
    {
        public ExecutionException()
        {
        }

        public ExecutionException(string message)
            : base(message)
        {
        }

        public ExecutionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
