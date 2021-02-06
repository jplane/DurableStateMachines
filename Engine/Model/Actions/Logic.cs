using DSM.Common;
using DSM.Common.Model.Actions;
using System.Diagnostics;
using System.Threading.Tasks;
using DSM.Engine;

namespace DSM.Engine.Model.Actions
{
    internal class Logic : Action
    {
        public Logic(ILogicMetadata metadata)
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
