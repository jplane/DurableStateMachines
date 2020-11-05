using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Diagnostics;

namespace CoreEngine.Model.Execution
{
    internal class Log : ExecutableContent
    {
        private readonly string _label;
        private readonly string _expression;

        public Log(XElement element)
            : base(element)
        {
            _label = element.Attribute("label")?.Value ?? string.Empty; ;

            _expression = element.Attribute("expr")?.Value ?? string.Empty;
        }

        public override void Execute(ExecutionContext context)
        {
            if (!string.IsNullOrWhiteSpace(_expression))
            {
                var message = context.Eval<string>(_expression);

                Trace.WriteLine(message);
            }
        }
    }
}
