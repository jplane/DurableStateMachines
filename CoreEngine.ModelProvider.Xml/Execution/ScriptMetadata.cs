using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.Execution
{
    public class ScriptMetadata : ExecutableContentMetadata, IScriptMetadata
    {
        private readonly Lazy<Func<dynamic, object>> _executor;

        public ScriptMetadata(XElement element)
            : base(element)
        {
            _executor = new Lazy<Func<dynamic, object>>(() =>
            {
                return ExpressionCompiler.Compile<object>(this.BodyExpression);
            });
        }

        private string BodyExpression => _element.Value ?? string.Empty;

        public void Execute(dynamic data)
        {
            _executor.Value(data);
        }
    }
}
