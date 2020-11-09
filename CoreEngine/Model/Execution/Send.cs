using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using CoreEngine.Model.DataManipulation;
using System.Threading.Tasks;
using Nito.AsyncEx;
using CoreEngine.Abstractions.Model.Execution.Metadata;

namespace CoreEngine.Model.Execution
{
    internal class Send : ExecutableContent
    {
        private readonly AsyncLazy<Content> _content;
        private readonly AsyncLazy<Param[]> _params;

        public Send(ISendMetadata metadata)
            : base(metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new AsyncLazy<Content>(async () =>
            {
                var meta = await metadata.GetContent();

                if (meta != null)
                    return new Content(meta);
                else
                    return null;
            });

            _params = new AsyncLazy<Param[]>(async () =>
            {
                return (await metadata.GetParams()).Select(pm => new Param(pm)).ToArray();
            });
        }

        protected override Task _Execute(ExecutionContext context)
        {
            if (!string.IsNullOrWhiteSpace(((ISendMetadata) _metadata).IdLocation))
            {
                var syntheticId = Guid.NewGuid().ToString("N");

                context.LogDebug($"Synthentic Id = {syntheticId}");

                context[((ISendMetadata) _metadata).IdLocation] = syntheticId;
            }

            try
            {
                return Task.CompletedTask;
            }
            catch(Exception ex)
            {
                context.EnqueueCommunicationError(ex);

                return Task.CompletedTask;
            }
        }
    }
}
