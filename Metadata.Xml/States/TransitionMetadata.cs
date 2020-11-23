using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.States
{
    public class TransitionMetadata : ITransitionMetadata
    {
        private readonly XElement _element;
        private readonly XAttribute _attribute;
        private readonly string _target = string.Empty;
        private readonly Lazy<Func<dynamic, bool>> _condition;
        private readonly Lazy<string> _uniqueId;

        public TransitionMetadata(XElement element)
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

            _uniqueId = new Lazy<string>(() =>
            {
                return element.GetUniqueElementPath();
            });
        }

        public TransitionMetadata(XAttribute attribute)
        {
            _attribute = attribute;

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
        }

        public TransitionMetadata(string target)
        {
            _target = target;

            _condition = new Lazy<Func<dynamic, bool>>(() => EvalTrue);
        }

        public string UniqueId => _uniqueId.Value;

        public virtual bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
        }

        public IEnumerable<string> Targets
        {
            get
            {
                var attr = _attribute;

                if (attr == null)
                {
                    attr = _element?.Attribute("target");
                }

                var value = attr?.Value ?? _target;

                if (string.IsNullOrWhiteSpace(value))
                {
                    return Enumerable.Empty<string>();
                }
                else
                {
                    return value.Split(" ");
                }
            }
        }

        public IEnumerable<string> Messages
        {
            get
            {
                var events = _element?.Attribute("event")?.Value;

                if (string.IsNullOrWhiteSpace(events))
                {
                    return Enumerable.Empty<string>();
                }
                else
                {
                    return events.Split(" ");
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

        private string ConditionExpr => _element?.Attribute("cond")?.Value ?? string.Empty;

        public TransitionType Type
        {
            get => (TransitionType) Enum.Parse(typeof(TransitionType),
                                               _element?.Attribute("type")?.Value ?? "external",
                                               true);
        }

        public IEnumerable<IExecutableContentMetadata> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            if (_element != null)
            {
                foreach (var node in _element.Elements())
                {
                    content.Add(ExecutableContentMetadata.Create(node));
                }
            }

            return content;
        }
    }
}
