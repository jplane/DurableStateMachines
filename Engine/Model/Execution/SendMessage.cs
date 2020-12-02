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

        protected override async Task _ExecuteAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (ISendMessageMetadata) _metadata;

            var delay = metadata.GetDelay(context.ScriptData);

            if (delay > TimeSpan.Zero)
            {
                await context.DelayAsync(delay);
            }

            await context.ExecuteContentAsync(metadata.UniqueId, async ec =>
            {
                Debug.Assert(ec != null);

                try
                {
                    var id = metadata.Id;

                    if (string.IsNullOrWhiteSpace(id))
                    {
                        id = Guid.NewGuid().ToString("N");

                        await ec.LogDebugAsync($"Synthentic Id = {id}");

                        if (!string.IsNullOrWhiteSpace(metadata.IdLocation))
                        {
                            ec.SetDataValue(metadata.IdLocation, id);
                        }
                    }

                    var type = metadata.GetType(ec.ScriptData);

                    Debug.Assert(!string.IsNullOrWhiteSpace(type));

                    var service = ec.GetExternalService(type);

                    Debug.Assert(service != null);

                    var target = metadata.GetTarget(ec.ScriptData);

                    var messageName = metadata.GetMessageName(ec.ScriptData);

                    var content = metadata.GetContent(ec.ScriptData);

                    var parms = metadata.GetParams(ec.ScriptData);

                    await service(target, messageName, content, id, parms);
                }
                catch(Exception ex)
                {
                    ec.EnqueueCommunicationError(ex);
                }
            });
        }
    }
}
