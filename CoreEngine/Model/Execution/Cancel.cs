using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Threading.Tasks;

namespace CoreEngine.Model.Execution
{
    internal class Cancel : ExecutableContent
    {
        public Cancel(ICancelMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _Execute(ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
