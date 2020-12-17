using Newtonsoft.Json.Linq;
using StateChartsDotNet.Metadata.Json;
using StateChartsDotNet.Metadata.Json.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace StateChartsDotNet.Metadata.Json.Services
{
    public class HttpPostMetadata : SendMessageMetadata
    {
        private readonly JObject _synthetic;

        internal HttpPostMetadata(JObject element)
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

            items.Add(new JProperty("type", "http-post"));

            items.Add(new JProperty("target", element.Property("url").Value));

            items.Add(new JProperty("content", element.Property("body").Value));

            var parms = new List<JObject>();

            foreach (var queryStringElement in element.Property("queryString")?.Value.Values<JObject>() ?? Enumerable.Empty<JObject>())
            {
                var name = new JProperty("name", $"?{queryStringElement.Property("name").Value}");

                var expr = new JProperty("expr", queryStringElement.Property("value").Value);

                var param = new JObject(name, expr);

                parms.Add(param);
            }

            foreach (var headerElement in element.Property("headers")?.Value.Values<JObject>() ?? Enumerable.Empty<JObject>())
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
