using System;
using System.Collections.Generic;
using System.Text;
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

        public override void Execute(ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
