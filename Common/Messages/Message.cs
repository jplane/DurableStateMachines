using System.Collections.Generic;

namespace StateChartsDotNet.Common.Messages
{
    public abstract class Message
    {
        internal Message(string name)
        {
            this.Name = name;
        }

        internal bool IsCancel => this.Name == "cancel";

        public string Name { get; }

        internal abstract MessageType Type { get; }
    }

    internal enum MessageType
    {
        Platform,
        Internal,
        External
    }
}