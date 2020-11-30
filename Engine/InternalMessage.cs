using StateChartsDotNet.Common.Messages;
using System.Collections.Generic;

namespace StateChartsDotNet
{
    internal class InternalMessage : Message
    {
        public InternalMessage(string name)
            : base(name)
        {
        }

        public override MessageType Type => MessageType.Internal;

        public object Content { get; set; }
    }
}
