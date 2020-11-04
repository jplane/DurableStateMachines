using System;

namespace CoreEngine
{
    public class Event
    {
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

        public object Data { get; set; }
    }

    internal enum EventType
    {
        Platform,
        Internal,
        External
    }
}