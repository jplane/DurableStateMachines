using DSM.Common;
using DSM.Common.Model.Actions;
using System.Threading.Tasks;
using DSM.Engine;

namespace DSM.Engine.Model.Actions
{
    internal class Raise : Action
    {
        public Raise(IRaiseMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            context.EnqueueInternal(((IRaiseMetadata) _metadata).GetMessage(context.ExecutionData));

            return Task.CompletedTask;
        }
    }
}
