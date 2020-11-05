using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace CoreEngine.Model.DataManipulation
{
    internal class Donedata
    {
        private readonly Lazy<Content> _content;
        private readonly Lazy<List<Param>> _params;

        public Donedata(XElement element)
        {
            _content = new Lazy<Content>(() =>
            {
                var node = element.ScxmlElement("content");

                return node == null ? null : new Content(node);
            });

            _params = new Lazy<List<Param>>(() =>
            {
                var nodes = element.ScxmlElements("param");

                return new List<Param>(nodes.Select(n => new Param(n)));
            });
        }
    }
}
