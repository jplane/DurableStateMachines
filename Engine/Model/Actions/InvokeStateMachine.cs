using System;
using System.Linq;
using System.Threading.Tasks;
using DSM.Common;
using System.Diagnostics;
using DSM.Common.Observability;
using DSM.Common.Model.Actions;

namespace DSM.Engine.Model.Actions
{
    internal class InvokeStateMachine : Action
    {
        private readonly Lazy<Action[]> _finalizeContent;

        public InvokeStateMachine(IInvokeStateMachineMetadata metadata)
            : base(metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _finalizeContent = new Lazy<Action[]>(() =>
            {
                return metadata.GetCompletionActions().Select(Action.Create).ToArray();
            });
        }

        protected async override Task _ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (IInvokeStateMachineMetadata) _metadata;

            var data = await context.InvokeChildStateMachine(metadata);

            Debug.Assert(data != null);

            if (!metadata.ResultLocation.Equals(default))
            {
                context.SetDataValue(metadata.ResultLocation, data);
            }

            foreach (var content in _finalizeContent.Value)
            {
                await content.ExecuteAsync(context);
            }

            context.EnqueueInternal($"done.invoke.{metadata.Id}");
        }
    }
}
