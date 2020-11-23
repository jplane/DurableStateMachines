using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.DataManipulation;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.DataManipulation
{
    public class DataInitMetadata : IDataInitMetadata
    {
        private readonly XElement _element;
        private readonly Lazy<Func<dynamic, object>> _getter;
        private readonly Lazy<string> _uniqueId;

        public DataInitMetadata(XElement element)
        {
            element.CheckArgNull(nameof(element));

            _element = element;

            _getter = new Lazy<Func<dynamic, object>>(() =>
            {
                if (!string.IsNullOrWhiteSpace(this.Expression))
                {
                    return ExpressionCompiler.Compile<object>(this.Expression);
                }
                else
                {
                    return ExpressionCompiler.Compile<object>(this.Body);
                }
            });

            _uniqueId = new Lazy<string>(() =>
            {
                return element.GetUniqueElementPath();
            });
        }

        public string UniqueId => _uniqueId.Value;

        public bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
        }

        public string Id => _element.Attribute("id").Value;

        public object GetValue(dynamic data)
        {
            return _getter.Value(data);
        }

        private string Expression => _element.Attribute("expr")?.Value ?? string.Empty;

        private string Body => _element.Value ?? string.Empty;
    }
}
