using System;
using System.Xml.Linq;

namespace CoreEngine.Model
{
    internal class Content
    {
        private readonly XElement _element;

        public Content(XElement element)
        {
            _element = element;
        }

        public void Execute(ExecutionContext context, StateChart statechart)
        {
        }
    }
}