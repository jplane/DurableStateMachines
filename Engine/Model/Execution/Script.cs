using DSM.Common;
using DSM.Common.Model.Execution;
using System.Diagnostics;
using System.Threading.Tasks;
using DSM.Engine;

namespace DSM.Engine.Model.Execution
{
    internal class Script : Action
    {
        public Script(ILogicMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (ILogicMetadata) _metadata;

            metadata.Execute(context.ExecutionData);

            return Task.CompletedTask;
        }
    }
}
