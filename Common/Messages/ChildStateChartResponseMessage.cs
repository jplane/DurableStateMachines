using System.Collections.Generic;

namespace StateChartsDotNet.Common.Messages
{
    public class ChildStateChartResponseMessage : ExternalMessage
    {
        public ChildStateChartResponseMessage(string name)
            : base(name)
        {
        }

        public bool IsDone => this.Name.StartsWith("done.invoke.");

        public string CorrelationId { get; set; }
    }
}
