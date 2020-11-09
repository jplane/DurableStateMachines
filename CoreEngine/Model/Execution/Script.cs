using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.Model.Execution
{
    internal class Script : ExecutableContent
    {
        public Script(IScriptMetadata metadata)
            : base(metadata)
        {
        }

        protected override async Task _Execute(ExecutionContext context)
        {
            if (!string.IsNullOrWhiteSpace(((IScriptMetadata) _metadata).BodyExpression))
            {
                await context.Eval<object>(((IScriptMetadata) _metadata).BodyExpression);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
