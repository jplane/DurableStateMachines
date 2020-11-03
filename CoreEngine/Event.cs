using System;

namespace CoreEngine
{
    internal class Event
    {
        public Event(string name, EventType type)
        {
            this.Name = name;
            this.Type = type;
            this.SendId = string.Empty;
            this.Origin = string.Empty;
            this.OriginType = string.Empty;
            this.InvokeId = string.Empty;
        }

        public bool IsCancel => this.Name == "cancel";

        public string Name { get; }

        public EventType Type { get; set; }

        public string SendId { get; set; }

        public string Origin { get; set; }

        public string OriginType { get; set; }
        
        public string InvokeId { get; set; }

        public object Data { get; set; }
    }

    internal enum EventType
    {
        Platform,
        Internal,
        External
    }
}