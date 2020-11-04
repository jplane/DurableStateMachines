using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.Model.Execution
{
    internal class Assign : ExecutableContent
    {
        private readonly string _location;
        private readonly string _expression;
        private readonly string _body;

        public Assign(XElement element)
        {
            _location = element.Attribute("location").Value;
            _expression = element.Attribute("expr")?.Value ?? string.Empty;
            _body = element.Value ?? string.Empty;
        }

        public override void Execute(ExecutionContext context)
        {
            if (!string.IsNullOrWhiteSpace(_expression))
            {
                context[_location] = context.Eval<object>(_expression);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
