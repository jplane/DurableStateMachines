using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.States
{
    public class HistoryStateMetadata : StateMetadata, IHistoryStateMetadata
    {
        internal HistoryStateMetadata(XElement element)
            : base(element)
        {
        }

        public override StateType Type => StateType.History;

        public bool IsDeep => bool.Parse(_element.Attribute("type")?.Value ?? "shallow");
    }
}
