using System;
using System.Collections.Generic;
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
        private readonly Lazy<List<ExecutableContent>> _content;

        public Foreach(XElement element)
            : base(element)
        {
            _arrayExpression = element.Attribute("array").Value;

            _item = element.Attribute("item").Value;

            _index = element.Attribute("index")?.Value ?? string.Empty;

            _content = new Lazy<List<ExecutableContent>>(() =>
            {
                var content = new List<ExecutableContent>();

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

            // TODO: needs to support setting a stack of context values

            for (var idx = 0; idx < shallowCopy.Length; idx++)
            {
                context.SetStateValue(_item, shallowCopy[idx]);

                if (!string.IsNullOrWhiteSpace(_index))
                {
                    context.SetStateValue(_index, idx);
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
                    context.SetStateValue(_item, null);

                    if (!string.IsNullOrWhiteSpace(_index))
                    {
                        context.SetStateValue(_index, null);
                    }
                }
            }
        }
    }
}
