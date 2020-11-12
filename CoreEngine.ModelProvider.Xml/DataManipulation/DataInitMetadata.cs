using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.DataManipulation
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
                    throw new NotImplementedException();
                }
            });
        }

        public string Id => _element.Attribute("id").Value;

        public async Task<object> GetValue(dynamic data)
        {
            return await (await _getter)(data);
        }

        private string Source => _element.Attribute("src")?.Value ?? string.Empty;

        private string Expression => _element.Attribute("expr")?.Value ?? string.Empty;

        private string Body => _element.Value ?? string.Empty;
    }
}
