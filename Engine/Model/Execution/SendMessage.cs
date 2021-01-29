using System;
using System.Linq;
using StateChartsDotNet.Common;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.Execution;
using System.Diagnostics;

namespace StateChartsDotNet.Model.Execution
{
    internal class SendMessage<TData> : ExecutableContent<TData>
    {
        public SendMessage(ISendMessageMetadata metadata)
            : base(metadata)
        {
        }

        protected override async Task _ExecuteAsync(ExecutionContextBase<TData> context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (ISendMessageMetadata) _metadata;

            try
            {
                if (metadata.Delay != null)
                {
                    await context.DelayAsync(metadata.Delay.Value);
                }

                var id = await context.ResolveSendMessageId(metadata);

                Debug.Assert(!string.IsNullOrWhiteSpace(id));

                await context.SendMessageAsync(metadata.ActivityType, id, metadata.Configuration);
            }
            catch (TaskCanceledException)
            {
                context.InternalCancel();
            }
            catch (Exception ex)
            {
                context.EnqueueCommunicationError(ex);
            }
        }
    }
}
