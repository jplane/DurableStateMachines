using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.Execution
{
    internal class Script : ExecutableContent
    {
        public Script(IScriptMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _ExecuteAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (IScriptMetadata) _metadata;

            return context.ExecuteContentAsync(metadata.UniqueId, ec =>
            {
                Debug.Assert(ec != null);

                metadata.Execute(ec.ScriptData);

                return Task.CompletedTask;
            });
        }
    }
}
