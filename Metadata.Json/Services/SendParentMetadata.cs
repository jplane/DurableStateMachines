using Newtonsoft.Json.Linq;
using StateChartsDotNet.Metadata.Json;
using StateChartsDotNet.Metadata.Json.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateChartsDotNet.Metadata.Json.Services
{
    public class SendParentMetadata : SendMessageMetadata
    {
        private readonly JObject _synthetic;

        internal SendParentMetadata(JObject element)
            : base(element)
        {
            var items = new List<object>();

            if (element.Property("id") != null)
            {
                items.Add(new JProperty("id", element.Property("id").Value));
            }
            else if (element.Property("idlocation") != null)
            {
                items.Add(new JProperty("idlocation", element.Property("idlocation").Value));
            }

            if (element.Property("delay") != null)
            {
                items.Add(new JProperty("delay", element.Property("delay").Value));
            }
            else if (element.Property("delayexpr") != null)
            {
                items.Add(new JProperty("delayexpr", element.Property("delayexpr").Value));
            }

            items.Add(new JProperty("type", "send-parent"));

            if (element.Property("messageName") != null)
            {
                items.Add(new JProperty("event", element.Property("messageName").Value));
            }
            else if (element.Property("messageNameExpr") != null)
            {
                items.Add(new JProperty("eventexpr", element.Property("messageNameExpr").Value));
            }

            if (element.Property("content") != null)
            {
                items.Add(new JProperty("content", element.Property("content").Value));
            }

            var parms = new List<JObject>();

            foreach (var headerElement in element.Property("parameters")?.Value.Values<JObject>() ?? Enumerable.Empty<JObject>())
            {
                var name = new JProperty("name", headerElement.Property("name").Value);

                var expr = new JProperty("expr", headerElement.Property("value").Value);

                var param = new JObject(name, expr);

                parms.Add(param);
            }

            items.Add(new JProperty("params", new JArray(parms)));

            _synthetic = new JObject(items);
        }

        protected override JObject Element => _synthetic;
    }
}
