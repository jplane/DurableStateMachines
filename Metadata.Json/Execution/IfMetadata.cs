using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;


namespace StateChartsDotNet.Metadata.Json.Execution
{
    public class IfMetadata : ExecutableContentMetadata, IIfMetadata
    {
        private readonly Lazy<Func<dynamic, bool>> _ifCondition;
        private readonly Lazy<Func<dynamic, bool>[]> _elseIfConditions;

        internal IfMetadata(JObject element)
            : base(element)
        {
            _ifCondition = new Lazy<Func<dynamic, bool>>(() =>
            {
                return ExpressionCompiler.Compile<bool>(this.IfConditionExpression);
            });

            _elseIfConditions = new Lazy<Func<dynamic, bool>[]>(() =>
            {
                var funcs = new List<Func<dynamic, bool>>();

                foreach (var condition in this.ElseIfConditionExpressions)
                {
                    funcs.Add(ExpressionCompiler.Compile<bool>(condition));
                }

                return funcs.ToArray();
            });
        }

        public bool EvalIfCondition(dynamic data)
        {
            return _ifCondition.Value(data);
        }

        public IEnumerable<Func<dynamic, bool>> GetElseIfConditions()
        {
            return _elseIfConditions.Value;
        }

        private string IfConditionExpression => _element.Property("cond").Value.Value<string>();

        private IEnumerable<string> ElseIfConditionExpressions
        {
            get
            {
                var nodes = _element.Property("elseif")?.Value.Value<JArray>();

                if (nodes != null)
                {
                    return nodes.Cast<JObject>().Select(n => n.Property("cond").Value.Value<string>());
                }
                else
                {
                    return Enumerable.Empty<string>();
                }
            }
        }

        public IEnumerable<IExecutableContentMetadata> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            var elements = _element.Property("content");

            if (elements != null)
            {
                foreach (var node in elements.Value.Values<JObject>())
                {
                    content.Add(ExecutableContentMetadata.Create(node));
                }
            }

            return content;
        }

        public IEnumerable<IEnumerable<IExecutableContentMetadata>> GetElseIfExecutableContent()
        {
            var nodes = _element.Property("elseif")?.Value.Value<JArray>();

            if (nodes != null)
            {
                var content = new List<IEnumerable<IExecutableContentMetadata>>();

                foreach (var node in nodes)
                {
                    var elements = ((JObject) node).Property("content").Value.Values<JObject>();

                    content.Add(elements.Select(e => ExecutableContentMetadata.Create(e)));
                }

                return content;
            }
            else
            {
                return Enumerable.Empty<IEnumerable<IExecutableContentMetadata>>();
            }
        }

        public IEnumerable<IExecutableContentMetadata> GetElseExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            var elseElement = _element.Property("else")?.Value.Value<JObject>();

            if (elseElement != null)
            {
                foreach (var node in elseElement.Property("content").Value.Values<JObject>())
                {
                    content.Add(ExecutableContentMetadata.Create(node));
                }
            }

            return content.AsEnumerable();
        }
    }
}
