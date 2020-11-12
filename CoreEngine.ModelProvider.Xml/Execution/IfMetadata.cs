using CoreEngine.Abstractions.Model.Execution.Metadata;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.Execution
{
    public class IfMetadata : ExecutableContentMetadata, IIfMetadata
    {
        private readonly AsyncLazy<Func<dynamic, Task<bool>>> _ifCondition;
        private readonly AsyncLazy<Func<dynamic, Task<bool>>[]> _elseIfConditions;

        public IfMetadata(XElement element)
            : base(element)
        {
            _ifCondition = new AsyncLazy<Func<dynamic, Task<bool>>>(async () =>
            {
                return await ExpressionCompiler.Compile<bool>(this.IfConditionExpression);
            });

            _elseIfConditions = new AsyncLazy<Func<dynamic, Task<bool>>[]>(async () =>
            {
                var funcs = new List<Func<dynamic, Task<bool>>>();

                foreach (var condition in this.ElseIfConditionExpressions)
                {
                    funcs.Add(await ExpressionCompiler.Compile<bool>(condition));
                }

                return funcs.ToArray();
            });
        }

        public async Task<bool> EvalIfCondition(dynamic data)
        {
            return await (await _ifCondition)(data);
        }

        public async Task<IEnumerable<Func<dynamic, Task<bool>>>> GetElseIfConditions()
        {
            return (await _elseIfConditions).AsEnumerable();
        }

        private string IfConditionExpression => _element.Attribute("cond").Value;

        private IEnumerable<string> ElseIfConditionExpressions
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
