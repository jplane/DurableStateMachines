using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;
using System.Collections;
using System.Diagnostics;

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
            element.CheckArgNull(nameof(element));

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
            context.CheckArgNull(nameof(context));

            var enumerable = context.Eval<IEnumerable>(_arrayExpression);

            Debug.Assert(enumerable != null);

            var shallowCopy = enumerable.OfType<object>().ToArray();

            // TODO: needs to support setting a stack of context values

            for (var idx = 0; idx < shallowCopy.Length; idx++)
            {
                context.SetDataValue(_item, shallowCopy[idx]);

                if (!string.IsNullOrWhiteSpace(_index))
                {
                    context.SetDataValue(_index, idx);
                }

                try
                {
                    foreach (var content in _content.Value)
                    {
                        content.Execute(context);
                    }
                }
                finally
                {
                    context.SetDataValue(_item, null);

                    if (!string.IsNullOrWhiteSpace(_index))
                    {
                        context.SetDataValue(_index, null);
                    }
                }
            }
        }
    }
}
