using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Metadata.Fluent.Execution;
using StateChartsDotNet.Metadata.Fluent.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace StateChartsDotNet.Metadata.Fluent.Services.HttpPost
{
    public static class HttpPostExtensions
    {
        public static SendMessageMetadata<OnEntryExitMetadata<TParent>> HttpPost<TParent>(this OnEntryExitMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.ActivityType("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<IfMetadata<TParent>> HttpPost<TParent>(this IfMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.ActivityType("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<ElseIfMetadata<TParent>> HttpPost<TParent>(this ElseIfMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.ActivityType("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<ElseMetadata<TParent>> HttpPost<TParent>(this ElseMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.ActivityType("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<ForeachMetadata<TParent>> HttpPost<TParent>(this ForeachMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.ActivityType("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<InvokeStateChartMetadata<TParent>> HttpPost<TParent>(this InvokeStateChartMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.ActivityType("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<TransitionMetadata<TParent>> HttpPost<TParent>(this TransitionMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var sendMessage = metadata.SendMessage;

            sendMessage.ActivityType("http-post");

            return sendMessage;
        }

        public static SendMessageMetadata<TParent> Url<TParent>(this SendMessageMetadata<TParent> metadata, string url)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            if (metadata.Configuration == null)
            {
                metadata.Configuration = new HttpSendMessageConfiguration();
            }

            ((HttpSendMessageConfiguration) metadata.Configuration).Uri = url;

            return metadata;
        }

        public static SendMessageMetadata<TParent> QueryStringNameValue<TParent>(this SendMessageMetadata<TParent> metadata,
                                                                                 string name,
                                                                                 string value)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            if (metadata.Configuration == null)
            {
                var config = new HttpSendMessageConfiguration();

                config.QueryString = new Dictionary<string, string>();
            }

            var queryStrings = (Dictionary<string, string>) ((HttpSendMessageConfiguration) metadata.Configuration).QueryString;

            queryStrings[name] = value;

            return metadata;
        }

        public static SendMessageMetadata<TParent> HeaderNameValue<TParent>(this SendMessageMetadata<TParent> metadata,
                                                                            string name,
                                                                            string value)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            if (metadata.Configuration == null)
            {
                var config = new HttpSendMessageConfiguration();

                config.Headers = new Dictionary<string, string>();
            }

            var headers = (Dictionary<string, string>) ((HttpSendMessageConfiguration) metadata.Configuration).Headers;

            headers[name] = value;

            return metadata;
        }

        public static SendMessageMetadata<TParent> Body<TParent>(this SendMessageMetadata<TParent> metadata, object body)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            if (metadata.Configuration == null)
            {
                metadata.Configuration = new HttpSendMessageConfiguration();
            }

            var config = (HttpSendMessageConfiguration) metadata.Configuration;

            config.Content = body;
            config.ContentExpressionType = HttpSendMessageContentType.StaticValue;
            config.ContentExpression = null;

            return metadata;
        }

        public static SendMessageMetadata<TParent> BodyExpr<TParent>(this SendMessageMetadata<TParent> metadata, string expression)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            if (metadata.Configuration == null)
            {
                metadata.Configuration = new HttpSendMessageConfiguration();
            }

            var config = (HttpSendMessageConfiguration) metadata.Configuration;

            config.Content = null;
            config.ContentExpression = expression;
            config.ContentExpressionType = HttpSendMessageContentType.CSharpExpression;

            return metadata;
        }

        public static SendMessageMetadata<TParent> BodyExpr<TParent>(this SendMessageMetadata<TParent> metadata,
                                                                     Expression<Func<IDictionary<string, object>, object>> expression)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));
            expression.CheckArgNull(nameof(expression));

            if (metadata.Configuration == null)
            {
                metadata.Configuration = new HttpSendMessageConfiguration();
            }

            var config = (HttpSendMessageConfiguration) metadata.Configuration;

            config.SetContentExpressionTree(expression);

            return metadata;
        }

        public static SendMessageMetadata<TParent> ContentType<TParent>(this SendMessageMetadata<TParent> metadata, string contentType)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            if (metadata.Configuration == null)
            {
                metadata.Configuration = new HttpSendMessageConfiguration();
            }

            ((HttpSendMessageConfiguration) metadata.Configuration).ContentType = contentType;

            return metadata;
        }
    }
}
