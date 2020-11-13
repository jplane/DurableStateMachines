using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Model.Execution
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

            context.EnqueueInternal(((IRaiseMetadata) _metadata).MessageName);

            return Task.CompletedTask;
        }
    }
}
