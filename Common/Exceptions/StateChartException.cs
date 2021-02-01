using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace DSM.Common.Exceptions
{
    public class StateMachineException : Exception
    {
        public StateMachineException()
        {
        }

        public StateMachineException(string message)
            : base(message)
        {
        }

        public StateMachineException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
