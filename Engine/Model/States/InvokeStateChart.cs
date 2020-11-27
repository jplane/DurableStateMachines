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
        private readonly string _parentId;
        private readonly IInvokeStateChartMetadata _metadata;
        private readonly Lazy<ExecutableContent[]> _finalizeContent;

        public InvokeStateChart(IInvokeStateChartMetadata metadata, State parent)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
            _parentId = parent.Id;

            _finalizeContent = new Lazy<ExecutableContent[]>(() =>
            {
                return metadata.GetFinalizeExecutableContent().Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            await context.LogInformationAsync("Start: InvokeStateChart.Execute");

            var invokeId = _metadata.Id;

            if (string.IsNullOrWhiteSpace(invokeId))
            {
                invokeId = $"{_parentId}.{Guid.NewGuid().ToString("N")}";

                await context.LogDebugAsync($"Synthentic Id = {invokeId}");

                if (!string.IsNullOrWhiteSpace(_metadata.IdLocation))
                {
                    context.SetDataValue(_metadata.IdLocation, invokeId);
                }
            }

            Debug.Assert(!string.IsNullOrWhiteSpace(invokeId));

            try
            {
                context.InvokeChildStateChart(_metadata, invokeId);
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
                foreach (var content in _finalizeContent.Value)
                {
                    await content.ExecuteAsync(context);
                }

                context.ProcessChildStateChartDone(response);
            }

            if (_metadata.Autoforward && !(externalMessage is ChildStateChartResponseMessage))
            {
                context.SendToChildStateChart(invokeId, externalMessage);
            }
        }
    }
}
