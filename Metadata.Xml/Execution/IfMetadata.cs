using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Execution
{
    public class IfMetadata : ExecutableContentMetadata, IIfMetadata
    {
        private readonly Lazy<Func<dynamic, bool>> _ifCondition;
        private readonly Lazy<Func<dynamic, bool>[]> _elseIfConditions;

        internal IfMetadata(XElement element)
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

        private string IfConditionExpression => _element.Attribute("cond").Value;

        private IEnumerable<string> ElseIfConditionExpressions
        {
            get
            {
                var nodes = _element.ScxmlElements("elseif");

                return nodes.Select(n => n.Attribute("cond").Value);
            }
        }

        public IEnumerable<IExecutableContentMetadata> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            foreach (var node in _element.Elements().Where(e => e.Name.LocalName != "elseif" &&
                                                                e.Name.LocalName != "else"))
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return content.AsEnumerable();
        }

        public IEnumerable<IEnumerable<IExecutableContentMetadata>> GetElseIfExecutableContent()
        {
            var content = new List<IEnumerable<IExecutableContentMetadata>>();

            foreach (var nodes in _element.ScxmlElements("elseif").Select(e => e.Elements()))
            {
                content.Add(nodes.Select(n => ExecutableContentMetadata.Create(n)));
            }

            return content;
        }

        public IEnumerable<IExecutableContentMetadata> GetElseExecutableContent()
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

            return content.AsEnumerable();
        }
    }
}
