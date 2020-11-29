using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Metadata.Fluent.Execution;
using StateChartsDotNet.Metadata.Fluent.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StateChartsDotNet.Metadata.Fluent.Services.HttpPost
{
    public static class HttpPostExtensions
    {
        public static SendMessageMetadata<OnEntryExitMetadata<TParent>> HttpPost<TParent>(this OnEntryExitMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<IfMetadata<TParent>> HttpPost<TParent>(this IfMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<ElseIfMetadata<TParent>> HttpPost<TParent>(this ElseIfMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<ElseMetadata<TParent>> HttpPost<TParent>(this ElseMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<ForeachMetadata<TParent>> HttpPost<TParent>(this ForeachMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<InvokeStateChartMetadata<TParent>> HttpPost<TParent>(this InvokeStateChartMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<TransitionMetadata<TParent>> HttpPost<TParent>(this TransitionMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage();

            sendMessage.Type("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<TParent> Url<TParent>(this SendMessageMetadata<TParent> metadata, string url)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Target(url);

            return metadata;
        }

        public static SendMessageMetadata<TParent> Url<TParent>(this SendMessageMetadata<TParent> metadata, Func<dynamic, string> getter)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Target(getter);

            return metadata;
        }

        public static SendMessageMetadata<TParent> Body<TParent>(this SendMessageMetadata<TParent> metadata, object body)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Content(body);

            return metadata;
        }

        public static SendMessageMetadata<TParent> Body<TParent>(this SendMessageMetadata<TParent> metadata, Func<dynamic, object> getter)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Content(getter);

            return metadata;
        }

        public static SendMessageMetadata<TParent> QueryStringNameValue<TParent>(this SendMessageMetadata<TParent> metadata,
                                                                                 string name,
                                                                                 string value)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var parm = metadata.Param($"?{name}");

            parm.Value(_ => value);

            return metadata;
        }

        public static SendMessageMetadata<TParent> QueryStringNameValue<TParent>(this SendMessageMetadata<TParent> metadata,
                                                                                 string name,
                                                                                 Func<dynamic, string> getter)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var parm = metadata.Param($"?{name}");

            parm.Value(getter);

            return metadata;
        }

        public static SendMessageMetadata<TParent> HeaderNameValue<TParent>(this SendMessageMetadata<TParent> metadata,
                                                                            string name,
                                                                            string value)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var parm = metadata.Param(name);

            parm.Value(_ => value);

            return metadata;
        }

        public static SendMessageMetadata<TParent> HeaderNameValue<TParent>(this SendMessageMetadata<TParent> metadata,
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
