using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.Execution
{
    internal class Script<TData> : ExecutableContent<TData>
    {
        public Script(IScriptMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _ExecuteAsync(ExecutionContextBase<TData> context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (IScriptMetadata) _metadata;

            metadata.Execute(context.ExecutionData);

            return Task.CompletedTask;
        }
    }
}
