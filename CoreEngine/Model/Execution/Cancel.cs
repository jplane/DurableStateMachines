using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CoreEngine.Model.Execution
{
    internal class Cancel : ExecutableContent
    {
        private readonly string _sendid;
        private readonly string _sendidExpr;

        public Cancel(XElement element)
        {
            _sendid = element.Attribute("sendid")?.Value ?? string.Empty;
            _sendidExpr = element.Attribute("sendidexpr")?.Value ?? string.Empty;
        }

        public override void Execute(ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
