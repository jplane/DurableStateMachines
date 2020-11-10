using CoreEngine.Abstractions.Model.Execution.Metadata;
using System.Threading.Tasks;

namespace CoreEngine.Model.Execution
{
    internal class Raise : ExecutableContent
    {
        public Raise(IRaiseMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.EnqueueInternal(((IRaiseMetadata) _metadata).Event);

            return Task.CompletedTask;
        }
    }
}
