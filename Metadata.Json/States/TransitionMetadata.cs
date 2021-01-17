using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Json.Execution;
using System;
using System.Collections.Generic;
using System.Linq;


namespace StateChartsDotNet.Metadata.Json.States
{
    public class TransitionMetadata : ITransitionMetadata
    {
        private readonly JObject _element;
        private readonly JProperty _prop;
        private readonly string _target = string.Empty;
        private readonly Lazy<Func<dynamic, bool>> _condition;
        private readonly string _metadataId;

        internal TransitionMetadata(JObject element)
        {
            _element = element;

            _condition = new Lazy<Func<dynamic, bool>>(() =>
            {
                if (string.IsNullOrWhiteSpace(this.ConditionExpr))
                {
                    return EvalTrue;
                }
                else
                {
                    return ExpressionCompiler.Compile<bool>(this.ConditionExpr);
                }
            });

            _metadataId = element.GetUniqueElementPath();
        }

        public TransitionMetadata(JProperty prop, string parentMetadataId)
        {
            _prop = prop;

            _condition = new Lazy<Func<dynamic, bool>>(() =>
            {
                if (string.IsNullOrWhiteSpace(this.ConditionExpr))
                {
                    return EvalTrue;
                }
                else
                {
                    return ExpressionCompiler.Compile<bool>(this.ConditionExpr);
                }
            });

            _metadataId = $"{parentMetadataId}.initialTransition";
        }

        public TransitionMetadata(string target, string parentMetadataId)
        {
            _target = target;

            _condition = new Lazy<Func<dynamic, bool>>(() => EvalTrue);

            _metadataId = $"{parentMetadataId}.initialTransition";
        }

        public string MetadataId => _metadataId;

        public bool BreakOnDebugger => false;

        public JObject DebugInfo => null;

        public IEnumerable<string> Targets
        {
            get
            {
                var attr = _prop?.Value.Value<string>();

                if (attr == null)
                {
                    attr = _element?.Property("target")?.Value.Value<string>();
                }

                var value = attr ?? _target;

                if (string.IsNullOrWhiteSpace(value))
                {
                    return Enumerable.Empty<string>();
                }
                else
                {
                    return value.Split(' ');
                }
            }
        }

        public IEnumerable<string> Messages
        {
            get
            {
                var events = _element?.Property("event")?.Value.Value<string>();

                if (string.IsNullOrWhiteSpace(events))
                {
                    return Enumerable.Empty<string>();
                }
                else
                {
                    return events.Split(' ');
                }
            }
        }

        public bool EvalCondition(dynamic data)
        {
            return _condition.Value(data);
        }

        private static bool EvalTrue(dynamic _)
        {
            return true;
        }

        private string ConditionExpr => _element?.Property("cond")?.Value.Value<string>() ?? string.Empty;

        public TransitionType Type
        {
            get => (TransitionType) Enum.Parse(typeof(TransitionType),
                                               _element?.Property("type")?.Value.Value<string>() ?? "external",
                                               true);
        }

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
