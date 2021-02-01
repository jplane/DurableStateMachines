using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace DSM.Common.Exceptions
{
    public class StateChartException : Exception
    {
        public StateChartException()
        {
        }

        public StateChartException(string message)
            : base(message)
        {
        }

        public StateChartException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
