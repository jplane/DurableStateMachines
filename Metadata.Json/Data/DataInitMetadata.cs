using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Metadata.Json.Data
{
    public class DataInitMetadata : IDataInitMetadata
    {
        private readonly JObject _element;
        private readonly Lazy<Func<dynamic, object>> _getter;
        private readonly Lazy<string> _uniqueId;

        internal DataInitMetadata(JObject element)
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

        public string Id => _element.Property("id").Value.Value<string>();

        public object GetValue(dynamic data)
        {
            return _getter.Value(data);
        }

        private string Expression => _element.Property("expr")?.Value.Value<string>() ?? string.Empty;

        private string Body => _element.Property("body")?.Value.Value<string>() ?? string.Empty;
    }
}
