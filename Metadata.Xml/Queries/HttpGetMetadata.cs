using StateChartsDotNet.Metadata.Xml;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Queries
{
    public class HttpGetMetadata : QueryMetadata
    {
        private readonly XElement _synthetic;

        internal HttpGetMetadata(XElement element)
            : base(element)
        {
            var items = new List<object>();

            if (element.Attribute("resultlocation") != null)
            {
                items.Add(new XAttribute("resultlocation", element.Attribute("resultlocation").Value));
            }

            items.Add(new XAttribute("type", "http-get"));

            items.Add(new XAttribute("target", element.ScxmlElement("url").Value));

            foreach (var queryStringElement in element.ScxmlElement("queryString")?.Elements() ?? Enumerable.Empty<XElement>())
            {
                var name = new XAttribute("name", $"?{queryStringElement.ScxmlElement("name").Value}");

                var expr = new XAttribute("expr", queryStringElement.ScxmlElement("value").Value);

                var param = new XElement(XName.Get("param", "http://www.w3.org/2005/07/scxml"), name, expr);

                items.Add(param);
            }

            foreach (var headerElement in element.ScxmlElement("headers")?.Elements() ?? Enumerable.Empty<XElement>())
            {
                var name = new XAttribute("name", headerElement.ScxmlElement("name").Value);

                var expr = new XAttribute("expr", headerElement.ScxmlElement("value").Value);

                var param = new XElement(XName.Get("param", "http://www.w3.org/2005/07/scxml"), name, expr);

                items.Add(param);
            }

            _synthetic = new XElement(XName.Get("query", "http://www.w3.org/2005/07/scxml"), items);
        }

        protected override XElement Element => _synthetic;
    }
}
