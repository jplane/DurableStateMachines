using System.Collections.Generic;

namespace DSM.Common.Messages
{
    public sealed class ExternalMessage : Message
    {
        public ExternalMessage()
        {
        }

        public override MessageType Type => MessageType.External;

        public object Content { get; set; }

        public string CorrelationId { get; set; }
    }
}
