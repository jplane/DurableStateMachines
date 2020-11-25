using System.Collections.Generic;

namespace StateChartsDotNet.Common.Messages
{
    public class ExternalMessage : Message
    {
        public ExternalMessage(string name)
            : base(name)
        {
        }

        internal override MessageType Type => MessageType.External;

        public object Data { get; set; }
    }
}
