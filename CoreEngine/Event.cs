using System;
using System.Collections.Generic;

namespace CoreEngine
{
    public class Event
    {
        private readonly Dictionary<string, object> _data =
            new Dictionary<string, object>();

        public Event(string name)
        {
            this.Name = name;
            this.SendId = string.Empty;
            this.Origin = string.Empty;
            this.OriginType = string.Empty;
            this.InvokeId = string.Empty;
        }

        internal bool IsCancel => this.Name == "cancel";

        public string Name { get; }

        internal EventType Type { get; set; }

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

    internal enum EventType
    {
        Platform,
        Internal,
        External
    }
}