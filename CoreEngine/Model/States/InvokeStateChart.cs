using StateChartsDotNet.CoreEngine.Model.DataManipulation;
using System;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using Nito.AsyncEx;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal class InvokeStateChart
    {
        private readonly IInvokeStateChart _metadata;
        private readonly string _parentId;
        private readonly AsyncLazy<Content> _content;
        private readonly AsyncLazy<Finalize> _finalize;
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

            _finalize = new AsyncLazy<Finalize>(async () =>
            {
                var meta = await metadata.GetFinalize();

                if (meta != null)
                    return new Finalize(meta);
                else
                    return null;
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

        public Task Execute(ExecutionContext context)
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
                context.EnqueueCommunicationError(ex);

                return Task.CompletedTask;
            }
            finally
            {
                context.LogInformation($"End: Invoke");
            }
        }

        public void Cancel(ExecutionContext context)
        {
        }

        public Task ProcessExternalEvent(ExecutionContext context, Event externalEvent)
        {
            externalEvent.CheckArgNull(nameof(externalEvent));

            var id = GetId(context);

            if (id == externalEvent.InvokeId)
            {
                ApplyFinalize(externalEvent);
            }

            if (_metadata.Autoforward)
            {
                // send events to service
            }

            return Task.CompletedTask;
        }

        private void ApplyFinalize(Event externalEvent)
        {
        }
    }
}
