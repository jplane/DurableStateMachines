using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.Model.Execution
{
    internal class Script : ExecutableContent
    {
        private readonly string _source;
        private readonly string _body;

        public Script(XElement element)
            : base(element)
        {
            element.CheckArgNull(nameof(element));

            _source = element.Attribute("src")?.Value ?? string.Empty;
            _body = element.Value ?? string.Empty;
        }

        protected override async Task _Execute(ExecutionContext context)
        {
            if (!string.IsNullOrWhiteSpace(_body))
            {
                await context.Eval<object>(_body);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
