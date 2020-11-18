using System;
using System.Linq;
using StateChartsDotNet.CoreEngine.Model.DataManipulation;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;

namespace StateChartsDotNet.CoreEngine.Model.Execution
{
    internal class SendMessage : ExecutableContent
    {
        private readonly Lazy<Content> _content;
        private readonly Lazy<Param[]> _params;

        public SendMessage(ISendMessageMetadata metadata)
            : base(metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new Lazy<Content>(() =>
            {
                var meta = metadata.GetContent();

                if (meta != null)
                    return new Content(meta);
                else
                    return null;
            });

            _params = new Lazy<Param[]>(() =>
            {
                return metadata.GetParams().Select(pm => new Param(pm)).ToArray();
            });
        }

        protected override Task _Execute(ExecutionContext context)
        {
            if (!string.IsNullOrWhiteSpace(((ISendMessageMetadata) _metadata).IdLocation))
            {
                var syntheticId = Guid.NewGuid().ToString("N");

                context.LogDebug($"Synthentic Id = {syntheticId}");

                context[((ISendMessageMetadata) _metadata).IdLocation] = syntheticId;
            }

            throw new NotImplementedException();
        }
    }
}
