﻿using System;
using System.Linq;
using DSM.Common;
using System.Threading.Tasks;
using DSM.Common.Model.Actions;
using DSM.Engine;

namespace DSM.Engine.Model.Actions
{
    internal class SendMessage : Action
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
                if (metadata.Delay != null)
                {
                    await context.DelayAsync(metadata.Delay.Value);
                }

                await context.SendMessageAsync(metadata.ActivityType, metadata.Id, metadata.GetConfiguration());
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
