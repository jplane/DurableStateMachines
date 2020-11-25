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

        internal override MessageType Type => MessageType.Internal;

        public object Data { get; set; }
    }
}
