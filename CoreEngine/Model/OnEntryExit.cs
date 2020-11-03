using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.Model
{
    internal class OnEntryExit
    {
        private readonly List<Lazy<Content>> _content;

        public OnEntryExit(XElement element)
        {
            var content = new List<Lazy<Content>>();

            foreach (var node in element.Elements())
            {
                content = content.Append(new Lazy<Content>(() => new Content(node)));
            }

            _content = content;
        }

        public void Execute(ExecutionContext context, StateChart statechart)
        {
            foreach (var content in _content)
            {
                content.Value.Execute(context, statechart);
            }
        }
    }
}
