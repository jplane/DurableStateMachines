using System;
using System.Xml.Linq;

namespace CoreEngine
{
    internal class ExecutableContent
    {
        private readonly XElement _element;

        public ExecutableContent(XElement element)
        {
            _element = element;
        }

        public void Execute(ExecutionContext context, StateChart statechart)
        {
        }
    }
}