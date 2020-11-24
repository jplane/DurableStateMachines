using StateChartsDotNet.Metadata.Xml;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace StateChartsDotNet.Services.Http
{
    public class XmlMetadata : SendMessageMetadata
    {
        private readonly XElement _synthetic;

        public XmlMetadata(XElement element)
            : base(element)
        {
            var items = new List<object>();

            if (element.Attribute("id") != null)
            {
                items.Add(new XAttribute("id", element.Attribute("id").Value));
            }
            else if (element.Attribute("idlocation") != null)
            {
                items.Add(new XAttribute("idlocation", element.Attribute("idlocation").Value));
            }

            if (element.Attribute("delay") != null)
            {
                items.Add(new XAttribute("delay", element.Attribute("delay").Value));
            }
            else if (element.Attribute("delayexpr") != null)
            {
                items.Add(new XAttribute("delayexpr", element.Attribute("delayexpr").Value));
            }

            items.Add(new XAttribute("type", "http-post"));

            items.Add(new XAttribute("target", element.ScxmlElement("url").Value));

            items.Add(new XElement(XName.Get("content", "http://www.w3.org/2005/07/scxml"), element.ScxmlElement("body").Value));

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

            _synthetic = new XElement(XName.Get("send", "http://www.w3.org/2005/07/scxml"), items);
        }

        protected override XElement Element => _synthetic;
    }
}
