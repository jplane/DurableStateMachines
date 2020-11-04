using System;
using SCG=System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace CoreEngine.Model.DataManipulation
{
    internal class Donedata
    {
        private readonly Lazy<Content> _content;
        private readonly Lazy<SCG.List<Param>> _params;

        public Donedata(XElement element)
        {
            _content = new Lazy<Content>(() =>
            {
                var node = element.Element("content");

                return node == null ? null : new Content(node);
            });

            _params = new Lazy<SCG.List<Param>>(() =>
            {
                var nodes = element.Elements("param");

                return new SCG.List<Param>(nodes.Select(n => new Param(n)));
            });
        }
    }
}
