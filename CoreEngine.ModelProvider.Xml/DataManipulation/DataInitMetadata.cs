using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.DataManipulation
{
    public class DataInitMetadata : IDataInitMetadata
    {
        private readonly XElement _element;
        private readonly AsyncLazy<Func<dynamic, Task<object>>> _getter;

        public DataInitMetadata(XElement element)
        {
            element.CheckArgNull(nameof(element));

            _element = element;

            _getter = new AsyncLazy<Func<dynamic, Task<object>>>(async () =>
            {
                if (!string.IsNullOrWhiteSpace(this.Expression))
                {
                    return await ExpressionCompiler.Compile<object>(this.Expression);
                }
                else
                {
                    return await ExpressionCompiler.Compile<object>(this.Body);
                }
            });
        }

        public string Id => _element.Attribute("id").Value;

        public async Task<object> GetValue(dynamic data)
        {
            return await (await _getter)(data);
        }

        private string Expression => _element.Attribute("expr")?.Value ?? string.Empty;

        private string Body => _element.Value ?? string.Empty;
    }
}
