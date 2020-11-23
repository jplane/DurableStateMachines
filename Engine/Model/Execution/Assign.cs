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

        protected override Task _ExecuteAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var assignMetadata = (IAssignMetadata) _metadata;

            var value = assignMetadata.GetValue(context.ScriptData);

            context.SetDataValue(assignMetadata.Location, value);

            context.LogDebugAsync($"Set {assignMetadata.Location} = {value}");

            return Task.CompletedTask;
        }
    }
}
