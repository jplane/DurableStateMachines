using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Execution
{
    public class AssignMetadata : ExecutableContentMetadata, IAssignMetadata
    {
        private readonly Lazy<Func<dynamic, object>> _getter;

        public AssignMetadata(XElement element)
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

        public string Location => _element.Attribute("location").Value;

        private string Expression => _element.Attribute("expr").Value ?? string.Empty;

        private string Body => _element.Value ?? string.Empty;
    }
}
