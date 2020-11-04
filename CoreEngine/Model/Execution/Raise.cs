using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.Model.Execution
{
    internal class Raise : ExecutableContent
    {
        private readonly string _event;

        public Raise(XElement element)
        {
            _event = element.Attribute("event").Value;
        }

        public override void Execute(ExecutionContext context)
        {
            context.EnqueueInternal(_event);
        }
    }
}
