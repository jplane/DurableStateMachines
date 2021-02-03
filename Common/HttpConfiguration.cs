using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSM.Common
{
    public abstract class HttpConfiguration
    {
        internal HttpConfiguration()
        {
        }

        public void ResolveConfigValues(Func<string, string> resolver)
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

    public sealed class HttpQueryConfiguration : HttpConfiguration
    {
    }

    public sealed class HttpSendMessageConfiguration : HttpConfiguration
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
                    return data => this.ContentFunction(data);
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
        private string ContentExpression { get; set; }

        public Func<dynamic, object> ContentFunction { get; set; }

        internal object GetContent(dynamic data) => _getContent.Value(data);
    }
}
