using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.Execution
{
    internal class Raise<TData> : ExecutableContent<TData>
    {
        public Raise(IRaiseMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _ExecuteAsync(ExecutionContextBase<TData> context)
        {
            context.CheckArgNull(nameof(context));

            context.EnqueueInternal(((IRaiseMetadata) _metadata).GetMessage(context.ExecutionData));

            return Task.CompletedTask;
        }
    }
}
