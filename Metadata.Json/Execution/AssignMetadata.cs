using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System;


namespace StateChartsDotNet.Metadata.Json.Execution
{
    public class AssignMetadata : ExecutableContentMetadata, IAssignMetadata
    {
        private readonly Lazy<Func<dynamic, object>> _getter;

        internal AssignMetadata(JObject element)
            : base(element)
        {
            _getter = new Lazy<Func<dynamic, object>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.Expression))
                {
                    return ExpressionCompiler.Compile<object>(this.Expression);
                }
                else
                {
                    throw new NotImplementedException();
                }
            });
        }

        public object GetValue(dynamic data)
        {
            return _getter.Value(data);
        }

        public string Location => _element.Property("location").Value.Value<string>();

        private string Expression => _element.Property("expr").Value.Value<string>() ?? string.Empty;

        private string Body => _element.Property("body").Value.Value<string>() ?? string.Empty;
    }
}
