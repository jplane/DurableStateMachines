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

        public HistoryType Type
        {
            get => (HistoryType) Enum.Parse(typeof(HistoryType),
                                            _element.Property("type")?.Value.Value<string>() ?? "shallow",
                                            true);
        }
    }
}
