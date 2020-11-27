using StateChartsDotNet.Metadata.Xml;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Services
{
    public class SendParentMetadata : SendMessageMetadata
    {
        private readonly XElement _synthetic;

        internal SendParentMetadata(XElement element)
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

            items.Add(new XAttribute("type", "send-parent"));

            if (element.ScxmlElement("messageName") != null)
            {
                items.Add(new XAttribute("event", element.ScxmlElement("messageName").Value));
            }
            else if (element.ScxmlElement("messageNameExpr") != null)
            {
                items.Add(new XAttribute("eventexpr", element.ScxmlElement("messageNameExpr").Value));
            }

            if (element.ScxmlElement("content") != null)
            {
                items.Add(element.ScxmlElement("content").Value);
            }

            foreach (var headerElement in element.ScxmlElement("parameters")?.Elements() ?? Enumerable.Empty<XElement>())
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
