using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Threading.Tasks;

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

            var assignMetadata = (IAssignMetadata) _metadata;

            var value = await assignMetadata.GetValue(context.ScriptData);

            context.SetDataValue(assignMetadata.Location, value);

            context.LogDebug($"Set {assignMetadata.Location} = {value}");
        }
    }
}
