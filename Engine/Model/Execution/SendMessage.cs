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

        protected override Task _ExecuteAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (ISendMessageMetadata) _metadata;

            return context.ExecuteContentAsync(metadata.UniqueId, async ec =>
            {
                Debug.Assert(ec != null);

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

                if (string.IsNullOrWhiteSpace(type))
                {
                    throw new InvalidOperationException("External service type not specified.");
                }

                var service = ec.GetExternalService(type);

                if (service == null)
                {
                    throw new InvalidOperationException($"External service '{type}' not configured.");
                }

                var target = metadata.GetTarget(ec.ScriptData);

                var messageName = metadata.GetMessageName(ec.ScriptData);

                var content = metadata.GetContent(ec.ScriptData);

                var parms = metadata.GetParams(ec.ScriptData);

                await service(target, messageName, content, id, parms);
            });
        }
    }
}
