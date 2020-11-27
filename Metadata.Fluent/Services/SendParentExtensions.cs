using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Metadata.Fluent.Execution;
using StateChartsDotNet.Metadata.Fluent.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StateChartsDotNet.Services.SendParent
{
    public static class SendParentExtensions
    {
        public static SendMessageMetadata<OnEntryExitMetadata<TParent>> SendParent<TParent>(this OnEntryExitMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("send-parent");

            return sendMessage;
        }

        public static SendMessageMetadata<IfMetadata<TParent>> SendParent<TParent>(this IfMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("send-parent");

            return sendMessage;
        }

        public static SendMessageMetadata<ElseIfMetadata<TParent>> SendParent<TParent>(this ElseIfMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("send-parent");

            return sendMessage;
        }

        public static SendMessageMetadata<ElseMetadata<TParent>> SendParent<TParent>(this ElseMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("send-parent");

            return sendMessage;
        }

        public static SendMessageMetadata<ForeachMetadata<TParent>> SendParent<TParent>(this ForeachMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("send-parent");

            return sendMessage;
        }

        public static SendMessageMetadata<InvokeStateChartMetadata<TParent>> SendParent<TParent>(this InvokeStateChartMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("send-parent");

            return sendMessage;
        }

        public static SendMessageMetadata<TransitionMetadata<TParent>> SendParent<TParent>(this TransitionMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("send-parent");

            return sendMessage;
        }

        public static SendMessageMetadata<TParent> MessageName<TParent>(this SendMessageMetadata<TParent> metadata, string messageName)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.MessageName(messageName);

            return metadata;
        }

        public static SendMessageMetadata<TParent> MessageName<TParent>(this SendMessageMetadata<TParent> metadata, Func<dynamic, string> getter)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.MessageName(getter);

            return metadata;
        }

        public static SendMessageMetadata<TParent> Content<TParent>(this SendMessageMetadata<TParent> metadata, object content)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Content(content);

            return metadata;
        }

        public static SendMessageMetadata<TParent> Content<TParent>(this SendMessageMetadata<TParent> metadata, Func<dynamic, object> getter)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Content(getter);

            return metadata;
        }

        public static SendMessageMetadata<TParent> Parameter<TParent>(this SendMessageMetadata<TParent> metadata,
                                                                      string name,
                                                                      string value)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var parm = metadata.Param(name);

            parm.Value(_ => value);

            return metadata;
        }

        public static SendMessageMetadata<TParent> Parameter<TParent>(this SendMessageMetadata<TParent> metadata,
                                                                      string name,
                                                                      Func<dynamic, string> getter)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var parm = metadata.Param(name);

            parm.Value(getter);

            return metadata;
        }
    }
}
