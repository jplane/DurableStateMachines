using StateChartsDotNet.Model.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Model.Execution;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using System.Diagnostics;

namespace StateChartsDotNet.Model.States
{
    internal class InvokeStateChart
    {
        private readonly IInvokeStateChartMetadata _metadata;
        private readonly Lazy<ExecutableContent[]> _finalizeContent;

        public InvokeStateChart(IInvokeStateChartMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;

            _finalizeContent = new Lazy<ExecutableContent[]>(() =>
            {
                return metadata.GetFinalizeExecutableContent().Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            await context.LogInformationAsync("Start: InvokeStateChart.Execute");

            try
            {
                await context.InvokeChildStateChart(_metadata);
            }
            catch (Exception ex)
            {
                context.EnqueueExecutionError(ex);
            }
            finally
            {
                await context.LogInformationAsync("End: InvokeStateChart.Execute");
            }
        }

        public async Task ProcessExternalMessageAsync(string invokeId, ExecutionContext context, ExternalMessage externalMessage)
        {
            invokeId.CheckArgNull(nameof(invokeId));
            context.CheckArgNull(nameof(context));
            externalMessage.CheckArgNull(nameof(externalMessage));

            if (externalMessage is ChildStateChartResponseMessage response && invokeId == response.CorrelationId)
            {
                // skip executing finalize executable content if we received an error and we're failing fast

                if (context.IsRunning)
                {
                    foreach (var content in _finalizeContent.Value)
                    {
                        await content.ExecuteAsync(context);
                    }
                }

                context.ProcessChildStateChartDone(response);
            }

            if (context.IsRunning)
            {
                if (_metadata.Autoforward && !(externalMessage is ChildStateChartResponseMessage))
                {
                    await context.SendToChildStateChart(invokeId, externalMessage);
                }
            }
        }
    }
}
