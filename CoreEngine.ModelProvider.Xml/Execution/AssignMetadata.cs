using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.Execution
{
    public class AssignMetadata : ExecutableContentMetadata, IAssignMetadata
    {
        private readonly AsyncLazy<Func<dynamic, Task<object>>> _getter;

        public AssignMetadata(XElement element)
            : base(element)
        {
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

        public async Task<object> GetValue(dynamic data)
        {
            return await (await _getter)(data);
        }

        public string Location => _element.Attribute("location").Value;

        private string Expression => _element.Attribute("expr").Value ?? string.Empty;

        private string Body => _element.Value ?? string.Empty;
    }
}
