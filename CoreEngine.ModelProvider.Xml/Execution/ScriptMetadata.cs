using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.Execution
{
    public class ScriptMetadata : ExecutableContentMetadata, IScriptMetadata
    {
        private readonly AsyncLazy<Func<dynamic, Task<object>>> _executor;

        public ScriptMetadata(XElement element)
            : base(element)
        {
            _executor = new AsyncLazy<Func<dynamic, Task<object>>>(async () =>
            {
                return await ExpressionCompiler.Compile<object>(this.BodyExpression);
            });
        }

        private string BodyExpression => _element.Value ?? string.Empty;

        public async Task Execute(dynamic data)
        {
            await (await _executor)(data);
        }
    }
}
