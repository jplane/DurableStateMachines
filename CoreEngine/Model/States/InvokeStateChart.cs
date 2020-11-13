using StateChartsDotNet.CoreEngine.Model.DataManipulation;
using System;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Model.Execution;
using StateChartsDotNet.CoreEngine.Abstractions;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal class InvokeStateChart
    {
        private readonly IInvokeStateChart _metadata;
        private readonly string _parentId;
        private readonly AsyncLazy<Content> _content;
        private readonly AsyncLazy<ExecutableContent[]> _finalizeContent;
        private readonly AsyncLazy<Param[]> _params;

        public InvokeStateChart(IInvokeStateChart metadata, State parent)
        {
            metadata.CheckArgNull(nameof(metadata));
            parent.CheckArgNull(nameof(parent));

            _metadata = metadata;
            _parentId = parent.Id;

            _content = new AsyncLazy<Content>(async () =>
            {
                var meta = await metadata.GetContent();

                if (meta != null)
                    return new Content(meta);
                else
                    return null;
            });

            _finalizeContent = new AsyncLazy<ExecutableContent[]>(async () =>
            {
                return (await metadata.GetFinalizeExecutableContent()).Select(ExecutableContent.Create).ToArray();
            });

            _params = new AsyncLazy<Param[]>(async () =>
            {
                return (await _metadata.GetParams()).Select(pm => new Param(pm)).ToArray();
            });
        }

        private string GetId(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            if (! string.IsNullOrWhiteSpace(_metadata.Id))
            {
                return _metadata.Id;
            }
            else if (string.IsNullOrWhiteSpace(_metadata.IdLocation) || !context.TryGet(_metadata.IdLocation, out object value))
            {
                throw new InvalidOperationException("Unable to resolve invoke ID.");
            }
            else
            {
                return (string) value;
            }
        }

        public async Task Execute(ExecutionContext context)
        {
            context.LogInformation($"Start: Invoke");

            if (!string.IsNullOrWhiteSpace(_metadata.IdLocation))
            {
                var syntheticId = $"{_parentId}.{Guid.NewGuid():N}";

                context.LogDebug($"Synthentic Id = {syntheticId}");

                context[_metadata.IdLocation] = syntheticId;
            }

            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                await context.EnqueueCommunicationError(ex);
            }
            finally
            {
                context.LogInformation($"End: Invoke");
            }
        }

        public void Cancel(ExecutionContext context)
        {
        }

        public Task ProcessExternalMessage(ExecutionContext context, Message externalMessage)
        {
            externalMessage.CheckArgNull(nameof(externalMessage));

            var id = GetId(context);

            if (id == externalMessage.InvokeId)
            {
                ApplyFinalize(externalMessage);
            }

            if (_metadata.Autoforward)
            {
                // send events to service
            }

            return Task.CompletedTask;
        }

        private void ApplyFinalize(Message externalMessage)
        {
        }
    }
}
