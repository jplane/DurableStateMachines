using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("StateChartsDotNet.CoreEngine")]

namespace StateChartsDotNet.CoreEngine.Abstractions
{
    public class Message
    {
        private readonly Dictionary<string, object> _data =
            new Dictionary<string, object>();

        public Message(string name)
        {
            this.Name = name;
            this.SendId = string.Empty;
            this.Origin = string.Empty;
            this.OriginType = string.Empty;
            this.InvokeId = string.Empty;
        }

        internal bool IsCancel => this.Name == "cancel";

        public string Name { get; }

        internal MessageType Type { get; set; }

        internal string SendId { get; set; }

        internal string Origin { get; set; }

        internal string OriginType { get; set; }

        internal string InvokeId { get; set; }

        public object this[string key]
        {
            get => _data[key];
            internal set => _data[key] = value;
        }
    }

    internal enum MessageType
    {
        Platform,
        Internal,
        External
    }
}