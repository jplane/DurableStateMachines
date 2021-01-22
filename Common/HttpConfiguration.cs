using Newtonsoft.Json;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace StateChartsDotNet.Common
{
    public abstract class HttpConfiguration
    {
        public virtual void ResolveConfigValues(Func<string, string> resolver)
        {
            resolver.CheckArgNull(nameof(resolver));

            this.Uri = resolver(this.Uri);

            this.Headers = this.Headers?.ToDictionary(pair => pair.Key, pair => resolver(pair.Value));

            this.QueryString = this.QueryString?.ToDictionary(pair => pair.Key, pair => resolver(pair.Value));
        }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("headers")]
        public IReadOnlyDictionary<string, string> Headers { get; set; }

        [JsonProperty("querystring")]
        public IReadOnlyDictionary<string, string> QueryString { get; set; }
    }

    public class HttpQueryConfiguration : HttpConfiguration, IQueryConfiguration
    {
    }

    public class HttpSendMessageConfiguration : HttpConfiguration, ISendMessageConfiguration
    {
        [JsonProperty("contenttype")]
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public object Content { get; set; }

        [JsonProperty("contentexpr")]
        public string ContentExpression { get; set; }

        [JsonProperty("contentexprtype")]
        public HttpSendMessageContentType ContentExpressionType { get; set; }

        internal void SetContentExpressionTree(LambdaExpression expression)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            var serializer = new ExpressionTreeBinarySerializer(writer);

            serializer.Visit(expression);

            this.Content = null;

            this.ContentExpression = Convert.ToBase64String(ms.ToArray());

            this.ContentExpressionType = HttpSendMessageContentType.ExpressionTree;
        }

        internal object GetContent(dynamic data)
        {
            object result = this.Content;

            switch (this.ContentExpressionType)
            {
                case HttpSendMessageContentType.CSharpExpression:
                    {
                        var func = ExpressionCompiler.Compile<object>(this.ContentExpression);

                        Debug.Assert(func != null);

                        result = func(data);
                    }
                    break;

                case HttpSendMessageContentType.ExpressionTree:
                    {
                        using var ms = new MemoryStream(Convert.FromBase64String(this.ContentExpression));
                        using var reader = new BinaryReader(ms);

                        var deserializer = new ExpressionTreeBinaryDeserializer(reader);

                        var lambdaExpr = deserializer.Visit();

                        Debug.Assert(lambdaExpr != null);

                        var func = (Func<IDictionary<string, object>, object>) lambdaExpr.Compile();

                        Debug.Assert(func != null);

                        result = func(data);
                    }
                    break;
            }

            return result;
        }
    }

    public enum HttpSendMessageContentType
    {
        StaticValue = 1,
        CSharpExpression,
        ExpressionTree
    }
}
