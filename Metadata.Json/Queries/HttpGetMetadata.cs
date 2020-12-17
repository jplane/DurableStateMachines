using Newtonsoft.Json.Linq;
using StateChartsDotNet.Metadata.Json;
using StateChartsDotNet.Metadata.Json.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace StateChartsDotNet.Metadata.Json.Queries
{
    public class HttpGetMetadata : QueryMetadata
    {
        private readonly JObject _synthetic;

        internal HttpGetMetadata(JObject element)
            : base(element)
        {
            var items = new List<object>();

            if (element.Property("resultlocation") != null)
            {
                items.Add(new JProperty("resultlocation", element.Property("resultlocation").Value));
            }

            items.Add(new JProperty("type", "http-get"));

            items.Add(new JProperty("target", element.Property("url").Value));

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
