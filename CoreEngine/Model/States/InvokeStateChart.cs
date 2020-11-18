using StateChartsDotNet.CoreEngine.Model.DataManipulation;
using System;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.Model.Execution;
using StateChartsDotNet.CoreEngine.Abstractions;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal class InvokeStateChart
    {
        private readonly IInvokeStateChart _metadata;
        private readonly string _parentId;
        private readonly Lazy<Content> _content;
        private readonly Lazy<ExecutableContent[]> _finalizeContent;
        private readonly Lazy<Param[]> _params;

        public InvokeStateChart(IInvokeStateChart metadata, State parent)
        {
            metadata.CheckArgNull(nameof(metadata));
            parent.CheckArgNull(nameof(parent));

            _metadata = metadata;
            _parentId = parent.Id;

            _content = new Lazy<Content>(() =>
            {
                var meta = metadata.GetContent();

                if (meta != null)
                    return new Content(meta);
                else
                    return null;
            });

            _finalizeContent = new Lazy<ExecutableContent[]>(() =>
            {
                return metadata.GetFinalizeExecutableContent().Select(ExecutableContent.Create).ToArray();
            });

            _params = new Lazy<Param[]>(() =>
            {
                return _metadata.GetParams().Select(pm => new Param(pm)).ToArray();
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

            return Task.CompletedTask;

            //try
            //{
            //    throw new NotImplementedException();
            //}
            //catch (Exception ex)
            //{
            //    context.EnqueueCommunicationError(ex);
            //}
            //finally
            //{
            //    context.LogInformation($"End: Invoke");
            //}
        }

        public Task Cancel(ExecutionContext context)
        {
            throw new NotImplementedException();
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
