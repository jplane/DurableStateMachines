using CoreEngine.Abstractions.Model;
using CoreEngine.Abstractions.Model.States.Metadata;
using System;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.States
{
    public class HistoryStateMetadata : StateMetadata, IHistoryStateMetadata
    {
        public HistoryStateMetadata(XElement element)
            : base(element)
        {
        }

        public HistoryType Type
        {
            get => (HistoryType) Enum.Parse(typeof(HistoryType),
                                            _element.Attribute("type")?.Value ?? "shallow",
                                            true);
        }
    }
}
