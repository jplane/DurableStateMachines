using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.Execution
{
    public class IfMetadata : ExecutableContentMetadata, IIfMetadata
    {
        public IfMetadata(XElement element)
            : base(element)
        {
        }

        public string IfConditionExpression => _element.Attribute("cond").Value;

        public IEnumerable<string> ElseIfConditionExpressions
        {
            get
            {
                var nodes = _element.ScxmlElements("elseif");

                return nodes.Select(n => n.Attribute("cond").Value);
            }
        }

        public Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            foreach (var node in _element.Elements().Where(e => e.Name.LocalName != "elseif" &&
                                                                e.Name.LocalName != "else"))
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return Task.FromResult(content.AsEnumerable());
        }

        public IEnumerable<Task<IEnumerable<IExecutableContentMetadata>>> GetElseIfExecutableContent()
        {
            var content = new List<Task<IEnumerable<IExecutableContentMetadata>>>();

            foreach (var nodes in _element.ScxmlElements("elseif").Select(e => e.Elements()))
            {
                content.Add(Task.FromResult(nodes.Select(n => ExecutableContentMetadata.Create(n))));
            }

            return content;
        }

        public Task<IEnumerable<IExecutableContentMetadata>> GetElseExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            var elseElement = _element.ScxmlElements("else").SingleOrDefault();

            if (elseElement != null)
            {
                foreach (var node in elseElement.Elements())
                {
                    content.Add(ExecutableContentMetadata.Create(node));
                }
            }

            return Task.FromResult(content.AsEnumerable());
        }
    }
}
