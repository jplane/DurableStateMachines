using DSM.Common;
using DSM.Common.Model.Execution;
using System.Diagnostics;
using System.Threading.Tasks;
using DSM.Engine;

namespace DSM.Engine.Model.Execution
{
    internal class Script : ExecutableContent
    {
        public Script(IScriptMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (IScriptMetadata) _metadata;

            metadata.Execute(context.ExecutionData);

            return Task.CompletedTask;
        }
    }
}
