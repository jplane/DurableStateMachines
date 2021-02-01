using System;
using System.Linq;
using System.Threading.Tasks;
using DSM.Common.Model.States;
using DSM.Engine.Model.Execution;
using DSM.Common;
using System.Diagnostics;
using DSM.Common.Debugger;
using DSM.Engine;

namespace DSM.Engine.Model.States
{
    internal class InvokeStateChart
    {
        private readonly IInvokeStateChartMetadata _metadata;
        private readonly string _parentMetadataId;
        private readonly Lazy<ExecutableContent[]> _finalizeContent;

        public InvokeStateChart(IInvokeStateChartMetadata metadata, string parentMetadataId)
        {
            metadata.CheckArgNull(nameof(metadata));
            parentMetadataId.CheckArgNull(nameof(parentMetadataId));

            _metadata = metadata;
            _parentMetadataId = parentMetadataId;

            _finalizeContent = new Lazy<ExecutableContent[]>(() =>
            {
                return metadata.GetFinalizeExecutableContent().Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            await context.LogInformationAsync("Start: InvokeStateChart.Execute");

            await context.BreakOnDebugger(DebuggerAction.BeforeInvokeChildStateMachine, _metadata);

            try
            {
                var data = await context.InvokeChildStateChart(_metadata, _parentMetadataId);

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
                await context.BreakOnDebugger(DebuggerAction.AfterInvokeChildStateMachine, _metadata);

                await context.LogInformationAsync("End: InvokeStateChart.Execute");
            }
        }
    }
}
