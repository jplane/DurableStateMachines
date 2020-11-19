using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.Execution
{
    public class ForeachMetadata : ExecutableContentMetadata, IForeachMetadata
    {
        private readonly Lazy<Func<dynamic, IEnumerable>> _arrayGetter;

        public ForeachMetadata(XElement element)
            : base(element)
        {
            _arrayGetter = new Lazy<Func<dynamic, IEnumerable>>(() =>
            {
                return ExpressionCompiler.Compile<IEnumerable>(this.ArrayExpression);
            });
        }

        public IEnumerable GetArray(dynamic data)
        {
            return _arrayGetter.Value(data);
        }

        private string ArrayExpression => _element.Attribute("array").Value;

        public string Item => _element.Attribute("item").Value;

        public string Index => _element.Attribute("index")?.Value ?? string.Empty;

        public IEnumerable<IExecutableContentMetadata> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            foreach (var node in _element.Elements())
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return content.AsEnumerable();
        }
    }
}
