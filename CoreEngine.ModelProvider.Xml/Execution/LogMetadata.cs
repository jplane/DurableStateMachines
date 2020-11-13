using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.Execution
{
    public class LogMetadata : ExecutableContentMetadata, ILogMetadata
    {
        private readonly AsyncLazy<Func<dynamic, Task<string>>> _messageGetter;

        public LogMetadata(XElement element)
            : base(element)
        {
            _messageGetter = new AsyncLazy<Func<dynamic, Task<string>>>(async () =>
            {
                return await ExpressionCompiler.Compile<string>(this.Message);
            });
        }

        private string Message => _element.Attribute("expr")?.Value ?? string.Empty;

        public async Task<string> GetMessage(dynamic data)
        {
            return await (await _messageGetter)(data);
        }
    }
}
