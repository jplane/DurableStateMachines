using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System;


namespace StateChartsDotNet.Metadata.Json.Execution
{
    public class LogMetadata : ExecutableContentMetadata, ILogMetadata
    {
        private readonly Lazy<Func<dynamic, string>> _messageGetter;

        internal LogMetadata(JObject element)
            : base(element)
        {
            _messageGetter = new Lazy<Func<dynamic, string>>(() =>
            {
                return ExpressionCompiler.Compile<string>(this.Message);
            });
        }

        private string Message => _element.Property("expr")?.Value.Value<string>() ?? string.Empty;

        public string GetMessage(dynamic data)
        {
            return _messageGetter.Value(data);
        }
    }
}
