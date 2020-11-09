using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.Model.Execution
{
    internal class Assign : ExecutableContent
    {
        public Assign(IAssignMetadata metadata)
            : base(metadata)
        {
        }

        protected override async Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            if (!string.IsNullOrWhiteSpace(((IAssignMetadata) _metadata).Expression))
            {
                var value = await context.Eval<object>(((IAssignMetadata) _metadata).Expression);

                context.SetDataValue(((IAssignMetadata) _metadata).Location, value);

                context.LogDebug($"Set {((IAssignMetadata) _metadata).Location} = {value}");
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
