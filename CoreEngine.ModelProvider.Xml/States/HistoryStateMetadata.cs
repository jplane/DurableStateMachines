using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using System;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.States
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
