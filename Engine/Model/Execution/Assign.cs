using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.Execution
{
    internal class Assign : ExecutableContent
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
