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

        public object Content { get; set; }

        public IReadOnlyDictionary<string, object> Parameters { get; set; }
    }
}
