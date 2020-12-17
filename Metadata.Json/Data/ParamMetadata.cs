using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Common.Model;
using System;
using System.Collections.Generic;


namespace StateChartsDotNet.Metadata.Json.Data
{
    public class ParamMetadata
    {
        private readonly string _location;
        private readonly string _expression;
        private readonly Lazy<Func<dynamic, object>> _getExpressionValue;
        private readonly Lazy<string> _uniqueId;

        internal ParamMetadata(JObject element)
        {
            this.Name = element.Property("name").Value.Value<string>();
            _location = element.Property("location")?.Value.Value<string>();
            _expression = element.Property("expr")?.Value.Value<string>();

            _getExpressionValue = new Lazy<Func<dynamic, object>>(() =>
            {
                return ExpressionCompiler.Compile<object>(_expression);
            });

            _uniqueId = new Lazy<string>(() =>
            {
                return element.GetUniqueElementPath();
            });
        }

        public ParamMetadata(string location)
        {
            this.Name = location;
            _location = location;
            _expression = null;
        }

        public bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
        }

        public string Name { get; }

        public object GetValue(dynamic data)
        {
            if (_location == null && _expression == null)
            {
                throw new MetadataValidationException("Param location or expression must be specified.");
            }
            else if (_location != null && _expression != null)
            {
                throw new MetadataValidationException("Only one of param location and expression can be specified.");
            }
            else if (_location != null)
            {
                return data[_location];
            }
            else
            {
                return _getExpressionValue.Value(data);
            }
        }
    }
}
