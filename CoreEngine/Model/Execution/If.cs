using System;
using SCG=System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace CoreEngine.Model.Execution
{
    internal class If : ExecutableContent
    {
        private readonly string _cond;
        private readonly Lazy<SCG.List<ElseIf>> _elseifs;
        private readonly Lazy<Else> _else;
        private readonly Lazy<SCG.List<ExecutableContent>> _content;

        public If(XElement element)
        {
            _cond = element.Attribute("cond").Value;

            _else = new Lazy<Else>(() =>
            {
                var node = element.Element("else");

                return node == null ? null : new Else(node);
            });

            _elseifs = new Lazy<SCG.List<ElseIf>>(() =>
            {
                var nodes = element.Elements("elseif");

                return new SCG.List<ElseIf>(nodes.Select(n => new ElseIf(n)));
            });

            _content = new Lazy<SCG.List<ExecutableContent>>(() =>
            {
                var content = new SCG.List<ExecutableContent>();

                foreach (var node in element.Elements())
                {
                    var item = ExecutableContent.Create(node);

                    if (item != null)
                    {
                        content.Add(item);
                    }
                }

                return content;
            });
        }

        public override void Execute(ExecutionContext context)
        {
            var result = context.Eval<bool>(_cond);

            if (result)
            {
                foreach (var content in _content.Value)
                {
                    content.Execute(context);
                }
            }
            else if (_elseifs.Value.Any(ei => ei.ConditionalExecute(context)))
            {
                return;
            }
            else if (_else.Value != null)
            {
                _else.Value.Execute(context);
            }
        }
    }
}
