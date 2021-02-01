using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace DSM.Common.Exceptions
{
    public sealed class CommunicationException : StateMachineException
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
