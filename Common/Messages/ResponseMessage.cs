using System.Collections.Generic;

namespace StateChartsDotNet.Common.Messages
{
    public class ResponseMessage : ExternalMessage
    {
        public ResponseMessage(string name)
            : base(name)
        {
        }

        public string CorrelationId { get; set; }
    }
}
