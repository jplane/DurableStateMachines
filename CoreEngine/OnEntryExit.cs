using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine
{
    internal class OnEntryExit
    {
        private readonly List<Lazy<ExecutableContent>> _content;

        public OnEntryExit(XElement element)
        {
            var content = new List<Lazy<ExecutableContent>>();

            foreach (var node in element.Elements())
            {
                content = content.Append(new Lazy<ExecutableContent>(() => new ExecutableContent(node)));
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
