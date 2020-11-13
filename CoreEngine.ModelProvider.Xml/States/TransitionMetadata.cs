using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml.Execution;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.States
{
    public class TransitionMetadata : ITransitionMetadata
    {
        private readonly XElement _element;
        private readonly XAttribute _attribute;
        private readonly string _target = string.Empty;
        private readonly AsyncLazy<Func<dynamic, Task<bool>>> _condition;

        public TransitionMetadata(XElement element)
        {
            _element = element;

            _condition = new AsyncLazy<Func<dynamic, Task<bool>>>(async () =>
            {
                if (string.IsNullOrWhiteSpace(this.ConditionExpr))
                {
                    return EvalTrue;
                }
                else
                {
                    return await ExpressionCompiler.Compile<bool>(this.ConditionExpr);
                }
            });
        }

        public TransitionMetadata(XAttribute attribute)
        {
            _attribute = attribute;

            _condition = new AsyncLazy<Func<dynamic, Task<bool>>>(async () =>
            {
                if (string.IsNullOrWhiteSpace(this.ConditionExpr))
                {
                    return EvalTrue;
                }
                else
                {
                    return await ExpressionCompiler.Compile<bool>(this.ConditionExpr);
                }
            });
        }

        public TransitionMetadata(string target)
        {
            _target = target;

            _condition = new AsyncLazy<Func<dynamic, Task<bool>>>(async () => EvalTrue);
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

        public async Task<bool> EvalCondition(dynamic data)
        {
            return await (await _condition)(data);
        }

        private static Task<bool> EvalTrue(dynamic _)
        {
            return Task.FromResult(true);
        }

        private string ConditionExpr => _element?.Attribute("cond")?.Value ?? string.Empty;

        public TransitionType Type
        {
            get => (TransitionType) Enum.Parse(typeof(TransitionType),
                                               _element?.Attribute("type")?.Value ?? "external",
                                               true);
        }

        public Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            if (_element != null)
            {
                foreach (var node in _element.Elements())
                {
                    content.Add(ExecutableContentMetadata.Create(node));
                }
            }

            return Task.FromResult(content.AsEnumerable());
        }
    }
}
