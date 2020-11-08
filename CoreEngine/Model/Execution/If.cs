using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace CoreEngine.Model.Execution
{
    internal class If : ExecutableContent
    {
        private readonly string _cond;
        private readonly Lazy<List<ElseIf>> _elseifs;
        private readonly Lazy<Else> _else;
        private readonly Lazy<List<ExecutableContent>> _content;

        public If(XElement element)
            : base(element)
        {
            element.CheckArgNull(nameof(element));

            _cond = element.Attribute("cond").Value;

            _else = new Lazy<Else>(() =>
            {
                var node = element.ScxmlElement("else");

                return node == null ? null : new Else(node);
            });

            _elseifs = new Lazy<List<ElseIf>>(() =>
            {
                var nodes = element.ScxmlElements("elseif");

                return new List<ElseIf>(nodes.Select(n => new ElseIf(n)));
            });

            _content = new Lazy<List<ExecutableContent>>(() =>
            {
                var content = new List<ExecutableContent>();

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

        protected override async Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var result = await context.Eval<bool>(_cond);

            context.LogDebug($"Condition = {result}");

            if (result)
            {
                foreach (var content in _content.Value)
                {
                    await content.Execute(context);
                }
            }
            else
            {
                foreach (var elseif in _elseifs.Value)
                {
                    if (await elseif.ConditionalExecute(context))
                    {
                        return;
                    }
                }

                if (_else.Value != null)
                {
                    await _else.Value.Execute(context);
                }
            }
        }
    }
}
