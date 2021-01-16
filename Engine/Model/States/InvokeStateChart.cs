using System;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Model.Execution;
using StateChartsDotNet.Common;

namespace StateChartsDotNet.Model.States
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

            try
            {
                await context.InvokeChildStateChart(_metadata, _parentMetadataId);

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
                await context.LogInformationAsync("End: InvokeStateChart.Execute");
            }
        }
    }
}
