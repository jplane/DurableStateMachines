using System.Collections.Generic;

namespace StateChartsDotNet.Common.Messages
{
    public abstract class Message
    {
        internal Message(string name)
        {
            name.CheckArgNull(nameof(name));
            this.Name = name;
        }

        public bool IsCancel => this.Name == "cancel";

        public string Name { get; }

        public abstract MessageType Type { get; }
    }

    public enum MessageType
    {
        Platform,
        Internal,
        External
    }
}