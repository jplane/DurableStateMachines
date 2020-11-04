using System;
using SCG=System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Collections;

namespace CoreEngine.Model.Execution
{
    internal class Foreach : ExecutableContent
    {
        private readonly string _arrayExpression;
        private readonly string _item;
        private readonly string _index;
        private readonly Lazy<SCG.List<ExecutableContent>> _content;

        public Foreach(XElement element)
        {
            _arrayExpression = element.Attribute("array").Value;

            _item = element.Attribute("item").Value;

            _index = element.Attribute("index")?.Value ?? string.Empty;

            _content = new Lazy<SCG.List<ExecutableContent>>(() =>
            {
                var content = new SCG.List<ExecutableContent>();

                foreach (var node in element.Elements())
                {
                    content.Add(ExecutableContent.Create(node));
                }

                return content;
            });
        }

        public override void Execute(ExecutionContext context)
        {
            var enumerable = context.Eval<IEnumerable>(_arrayExpression);

            var shallowCopy = enumerable.OfType<object>().ToArray();

            for (var idx = 0; idx < shallowCopy.Length; idx++)
            {
                context[_item] = shallowCopy[idx];

                if (!string.IsNullOrWhiteSpace(_index))
                {
                    context[_index] = idx;
                }

                try
                {
                    foreach (var content in _content.Value)
                    {
                        content.Execute(context);
                    }
                }
                catch (Exception ex)
                {
                    context.EnqueueInternal("error.execution");
                }
                finally
                {
                    context[_item] = null;

                    if (!string.IsNullOrWhiteSpace(_index))
                    {
                        context[_index] = null;
                    }
                }
            }
        }
    }
}
