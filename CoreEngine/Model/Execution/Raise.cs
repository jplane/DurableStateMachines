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
            : base(element)
        {
            element.CheckArgNull(nameof(element));

            _event = element.Attribute("event").Value;
        }

        public override void Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.EnqueueInternal(_event);
        }
    }
}
