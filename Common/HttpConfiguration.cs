using Newtonsoft.Json;
using StateChartsDotNet.Common.ExpressionTrees;
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
        private readonly Lazy<Func<dynamic, object>> _getContent;

        public HttpSendMessageConfiguration()
        {
            _getContent = new Lazy<Func<dynamic, object>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.ContentExpression))
                {
                    return ExpressionCompiler.Compile<object>(this.ContentExpression);
                }
                else if (this.ContentFunction != null)
                {
                    var func = this.ContentFunction.Compile();

                    Debug.Assert(func != null);

                    return data => func((IDictionary<string, object>) data);
                }
                else
                {
                    return _ => this.Content;
                }
            });
        }

        [JsonProperty("contenttype")]
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public object Content { get; set; }

        [JsonProperty("contentexpression")]
        public string ContentExpression { get; set; }

        [JsonProperty("contentfunction", ItemConverterType = typeof(ExpressionTreeConverter))]
        public Expression<Func<IDictionary<string, object>, object>> ContentFunction { get; set; }

        internal object GetContent(dynamic data) => _getContent.Value(data);
    }
}
