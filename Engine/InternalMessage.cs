using StateChartsDotNet.Common.Messages;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet
{
    public class InternalMessage : Message
    {
        internal InternalMessage(string name)
            : base(name)
        {
        }

        public bool IsError => this.Content != null && this.Content is Exception;

        public override MessageType Type => MessageType.Internal;

        public object Content { get; set; }
    }
}
