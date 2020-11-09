using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.Model.Execution
{
    internal class Raise : ExecutableContent
    {
        public Raise(IRaiseMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.EnqueueInternal(((IRaiseMetadata) _metadata).Event);

            return Task.CompletedTask;
        }
    }
}
