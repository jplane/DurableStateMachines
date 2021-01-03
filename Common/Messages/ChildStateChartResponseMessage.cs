﻿using System.Collections.Generic;

namespace StateChartsDotNet.Common.Messages
{
    public class ChildStateChartResponseMessage : ExternalMessage
    {
        public ChildStateChartResponseMessage()
        {
        }

        public bool IsDone => this.Name.StartsWith("done.invoke.");

        public bool IsInvokeError => this.Name.StartsWith("done.invoke.error.");

        public string CorrelationId { get; set; }
    }
}
