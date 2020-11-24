using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Execution
{
    public class LogMetadata : ExecutableContentMetadata, ILogMetadata
    {
        private readonly Lazy<Func<dynamic, string>> _messageGetter;

        internal LogMetadata(XElement element)
            : base(element)
        {
            _messageGetter = new Lazy<Func<dynamic, string>>(() =>
            {
                return ExpressionCompiler.Compile<string>(this.Message);
            });
        }

        private string Message => _element.Attribute("expr")?.Value ?? string.Empty;

        public string GetMessage(dynamic data)
        {
            return _messageGetter.Value(data);
        }
    }
}
