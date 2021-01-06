using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.States;
using System;

namespace StateChartsDotNet.Metadata.Json.States
{
    public class HistoryStateMetadata : StateMetadata, IHistoryStateMetadata
    {
        internal HistoryStateMetadata(JObject element)
            : base(element)
        {
        }

        public override StateType Type => StateType.History;

        public bool IsDeep => bool.Parse(_element.Property("deep")?.Value.Value<string>() ?? "false");
    }
}
