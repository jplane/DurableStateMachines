using System;
using System.Linq;
using StateChartsDotNet.Model.DataManipulation;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.Execution;
using System.Diagnostics;

namespace StateChartsDotNet.Model.Execution
{
    internal class SendMessage : ExecutableContent
    {
        public SendMessage(ISendMessageMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _ExecuteAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (ISendMessageMetadata) _metadata;

            return context.ExecuteContentAsync(metadata.UniqueId, async ec =>
            {
                Debug.Assert(ec != null);

                if (!string.IsNullOrWhiteSpace(metadata.IdLocation))
                {
                    var syntheticId = Guid.NewGuid().ToString("N");

                    await ec.LogDebugAsync($"Synthentic Id = {syntheticId}");

                    ec.SetDataValue(metadata.IdLocation, syntheticId);
                }

                throw new NotImplementedException();
            });
        }
    }
}
