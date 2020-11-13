using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using Nito.AsyncEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.Execution
{
    public class ForeachMetadata : ExecutableContentMetadata, IForeachMetadata
    {
        private readonly AsyncLazy<Func<dynamic, Task<IEnumerable>>> _arrayGetter;

        public ForeachMetadata(XElement element)
            : base(element)
        {
            _arrayGetter = new AsyncLazy<Func<dynamic, Task<IEnumerable>>>(async () =>
            {
                return await ExpressionCompiler.Compile<IEnumerable>(this.ArrayExpression);
            });
        }

        public async Task<IEnumerable> GetArray(dynamic data)
        {
            return await (await _arrayGetter)(data);
        }

        private string ArrayExpression => _element.Attribute("array").Value;

        public string Item => _element.Attribute("item").Value;

        public string Index => _element.Attribute("index")?.Value ?? string.Empty;

        public Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            foreach (var node in _element.Elements())
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return Task.FromResult(content.AsEnumerable());
        }
    }
}
