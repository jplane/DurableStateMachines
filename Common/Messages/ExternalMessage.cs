using System.Collections.Generic;

namespace StateChartsDotNet.Common.Messages
{
    public class ExternalMessage : Message
    {
        public ExternalMessage()
        {
        }

        public override MessageType Type => MessageType.External;

        public object Content { get; set; }

        public IReadOnlyDictionary<string, object> Parameters { get; set; }

        public bool IsDone => this.Name.StartsWith("done.invoke.");

        public bool IsInvokeError => this.Name.StartsWith("done.invoke.error.");

        public string CorrelationId { get; set; }

        public bool IsChildStateChartResponse => ! string.IsNullOrWhiteSpace(this.CorrelationId);
    }
}
