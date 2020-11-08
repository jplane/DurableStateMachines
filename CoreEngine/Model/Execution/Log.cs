using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CoreEngine.Model.Execution
{
    internal class Log : ExecutableContent
    {
        private readonly string _label;
        private readonly string _expression;

        public Log(XElement element)
            : base(element)
        {
            element.CheckArgNull(nameof(element));

            _label = element.Attribute("label")?.Value ?? string.Empty; ;

            _expression = element.Attribute("expr")?.Value ?? string.Empty;
        }

        protected override async Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            if (!string.IsNullOrWhiteSpace(_expression))
            {
                var message = await context.Eval<string>(_expression);

                context.LogInformation("Log: " + message);
            }
        }
    }
}
