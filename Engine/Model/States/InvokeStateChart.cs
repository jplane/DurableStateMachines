using System;
using System.Linq;
using System.Threading.Tasks;
using DSM.Common.Model.States;
using DSM.Engine.Model.Execution;
using DSM.Common;
using System.Diagnostics;
using DSM.Common.Observability;

namespace DSM.Engine.Model.States
{
    internal class InvokeStateMachine
    {
        private readonly IInvokeStateMachineMetadata _metadata;
        private readonly Lazy<ExecutableContent[]> _finalizeContent;

        public InvokeStateMachine(IInvokeStateMachineMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;

            _finalizeContent = new Lazy<ExecutableContent[]>(() =>
            {
                return metadata.GetFinalizeExecutableContent().Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            await context.LogInformationAsync("Start: InvokeStateMachine.Execute");

            await context.OnAction(ObservableAction.BeforeInvokeChildStateMachine, _metadata);

            try
            {
                var data = await context.InvokeChildStateMachine(_metadata);

                Debug.Assert(data != null);

                if (!_metadata.ResultLocation.Equals(default))
                {
                    context.SetDataValue(_metadata.ResultLocation, data);
                }

                foreach (var content in _finalizeContent.Value)
                {
                    await content.ExecuteAsync(context);
                }

                context.EnqueueInternal($"done.invoke.{_metadata.Id}");
            }
            catch (Exception ex)
            {
                context.EnqueueExecutionError(ex);
            }
            finally
            {
                await context.OnAction(ObservableAction.AfterInvokeChildStateMachine, _metadata);

                await context.LogInformationAsync("End: InvokeStateMachine.Execute");
            }
        }
    }
}
