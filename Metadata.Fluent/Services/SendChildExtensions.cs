using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Metadata.Fluent.Execution;
using StateChartsDotNet.Metadata.Fluent.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StateChartsDotNet.Metadata.Fluent.Services.SendChild
{
    public static class SendChildExtensions
    {
        public static SendMessageMetadata<OnEntryExitMetadata<TParent>> SendChild<TParent>(this OnEntryExitMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.Type("send-child");

            return sendMessage;
        }

        public static SendMessageMetadata<IfMetadata<TParent>> SendChild<TParent>(this IfMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.Type("send-child");

            return sendMessage;
        }

        public static SendMessageMetadata<ElseIfMetadata<TParent>> SendChild<TParent>(this ElseIfMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.Type("send-child");

            return sendMessage;
        }

        public static SendMessageMetadata<ElseMetadata<TParent>> SendChild<TParent>(this ElseMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.Type("send-child");

            return sendMessage;
        }

        public static SendMessageMetadata<ForeachMetadata<TParent>> SendChild<TParent>(this ForeachMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.Type("send-child");

            return sendMessage;
        }

        public static SendMessageMetadata<InvokeStateChartMetadata<TParent>> SendChild<TParent>(this InvokeStateChartMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.Type("send-child");

            return sendMessage;
        }

        public static SendMessageMetadata<TransitionMetadata<TParent>> SendChild<TParent>(this TransitionMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.Type("send-child");

            return sendMessage;
        }

        public static SendMessageMetadata<TParent> Child<TParent>(this SendMessageMetadata<TParent> metadata, string childId)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Target(childId);

            return metadata;
        }

        public static SendMessageMetadata<TParent> Child<TParent>(this SendMessageMetadata<TParent> metadata, Func<dynamic, string> getter)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Target(getter);

            return metadata;
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

            parm.Value(value);

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
