using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System;


namespace StateChartsDotNet.Metadata.Json.Execution
{
    public class ScriptMetadata : ExecutableContentMetadata, IScriptMetadata
    {
        private readonly Lazy<Func<dynamic, object>> _executor;

        internal ScriptMetadata(JObject element)
            : base(element)
        {
            _executor = new Lazy<Func<dynamic, object>>(() =>
            {
                return ExpressionCompiler.Compile<object>(this.BodyExpression);
            });
        }

        private string BodyExpression => _element.Property("body").Value.Value<string>() ?? string.Empty;

        public void Execute(dynamic data)
        {
            _executor.Value(data);
        }
    }
}
