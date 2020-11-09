using CoreEngine.Abstractions.Model;
using CoreEngine.Abstractions.Model.Execution.Metadata;
using CoreEngine.Abstractions.Model.States.Metadata;
using CoreEngine.ModelProvider.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.States
{
    public class TransitionMetadata : ITransitionMetadata
    {
        private readonly XElement _element;
        private readonly XAttribute _attribute;
        private readonly string _target = string.Empty;

        public TransitionMetadata(XElement element)
        {
            _element = element;
        }

        public TransitionMetadata(XAttribute attribute)
        {
            _attribute = attribute;
        }

        public TransitionMetadata(string target)
        {
            _target = target;
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

        public IEnumerable<string> Events
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

        public string ConditionExpr => _element?.Attribute("cond")?.Value ?? string.Empty;

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
