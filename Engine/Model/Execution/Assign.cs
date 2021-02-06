using DSM.Common;
using DSM.Common.Model.Execution;
using DSM.Engine;
using System.Threading.Tasks;

namespace DSM.Engine.Model.Execution
{
    internal class Assign : Action
    {
        public Assign(IAssignMetadata metadata)
            : base(metadata)
        {
        }

        protected override async Task _ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            var assignMetadata = (IAssignMetadata) _metadata;

            var value = assignMetadata.GetValue(context.ExecutionData);

            context.SetDataValue(assignMetadata.Location, value);

            await context.LogDebugAsync($"Set {assignMetadata.Location} = {value}");
        }
    }
}
