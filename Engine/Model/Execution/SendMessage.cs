using System;
using System.Linq;
using StateChartsDotNet.Common;
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

        protected override async Task _ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (ISendMessageMetadata) _metadata;

            try
            {
                var delay = metadata.GetDelay(context.ScriptData);

                if (delay > TimeSpan.Zero)
                {
                    await context.DelayAsync(delay);
                }

                var id = await context.ResolveSendMessageId(metadata);

                var type = metadata.GetType(context.ScriptData);

                Debug.Assert(!string.IsNullOrWhiteSpace(type));

                var target = metadata.GetTarget(context.ScriptData);

                Debug.Assert(!string.IsNullOrWhiteSpace(target));

                var messageName = metadata.GetMessageName(context.ScriptData);

                var content = metadata.GetContent(context.ScriptData);

                var parms = metadata.GetParams(context.ScriptData);

                Debug.Assert(parms != null);

                await context.SendMessageAsync(type, target, messageName, content, id, parms);
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
