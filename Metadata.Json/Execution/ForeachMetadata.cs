using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StateChartsDotNet.Metadata.Json.Execution
{
    public class ForeachMetadata : ExecutableContentMetadata, IForeachMetadata
    {
        private readonly Lazy<Func<dynamic, IEnumerable>> _arrayGetter;

        internal ForeachMetadata(JObject element)
            : base(element)
        {
            _arrayGetter = new Lazy<Func<dynamic, IEnumerable>>(() =>
            {
                return ExpressionCompiler.Compile<IEnumerable>(this.ArrayExpression);
            });
        }

        public IEnumerable GetArray(dynamic data)
        {
            return _arrayGetter.Value(data);
        }

        private string ArrayExpression => _element.Property("array").Value.Value<string>();

        public string Item => _element.Property("item").Value.Value<string>();

        public string Index => _element.Property("index")?.Value.Value<string>() ?? string.Empty;

        public IEnumerable<IExecutableContentMetadata> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            if (_element != null)
            {
                var elements = _element.Property("content");

                if (elements != null)
                {
                    foreach (var node in elements.Value.Values<JObject>())
                    {
                        content.Add(ExecutableContentMetadata.Create(node));
                    }
                }
            }

            return content;
        }
    }
}
