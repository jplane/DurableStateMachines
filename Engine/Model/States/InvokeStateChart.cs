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
        private readonly string _parentUniqueId;
        private readonly Lazy<ExecutableContent[]> _finalizeContent;

        public InvokeStateChart(IInvokeStateChartMetadata metadata, string parentUniqueId)
        {
            metadata.CheckArgNull(nameof(metadata));
            parentUniqueId.CheckArgNull(nameof(parentUniqueId));

            _metadata = metadata;
            _parentUniqueId = parentUniqueId;

            _finalizeContent = new Lazy<ExecutableContent[]>(() =>
            {
                return metadata.GetFinalizeExecutableContent().Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            await context.LogInformationAsync("Start: InvokeStateChart.Execute");

            try
            {
                await context.InvokeChildStateChart(_metadata, _parentUniqueId);
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

        public async Task ProcessExternalMessageAsync(string invokeId, ExecutionContextBase context, ExternalMessage externalMessage)
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

                await context.ProcessChildStateChartDoneAsync(response);
            }

            if (context.IsRunning &&
                _metadata.Autoforward &&
                !(externalMessage is ChildStateChartResponseMessage))
            {
                await context.SendToChildStateChart(invokeId, externalMessage);
            }
        }
    }
}
