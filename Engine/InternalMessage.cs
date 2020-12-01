using StateChartsDotNet.Common.Messages;
using System.Collections.Generic;

namespace StateChartsDotNet
{
    public class InternalMessage : Message
    {
        internal InternalMessage(string name)
            : base(name)
        {
        }

        public override MessageType Type => MessageType.Internal;

        public object Content { get; set; }
    }
}
