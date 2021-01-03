using System.Collections.Generic;

namespace StateChartsDotNet.Common.Messages
{
    public abstract class Message
    {
        internal Message()
        {
        }

        public bool IsCancel => this.Name == "cancel";

        public string Name { get; set; }

        public abstract MessageType Type { get; }
    }

    public enum MessageType
    {
        Platform,
        Internal,
        External
    }
}