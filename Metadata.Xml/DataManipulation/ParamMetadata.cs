using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.DataManipulation;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.DataManipulation
{
    public class ParamMetadata
    {
        private readonly string _location;
        private readonly string _expression;
        private readonly Lazy<Func<dynamic, object>> _getExpressionValue;
        private readonly Lazy<string> _uniqueId;

        public ParamMetadata(XElement element)
        {
            this.Name = element.Attribute("name").Value;
            _location = element.Attribute("location")?.Value ?? string.Empty;
            _expression = element.Attribute("expr")?.Value ?? string.Empty;

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

            if (string.IsNullOrWhiteSpace(_location) && string.IsNullOrWhiteSpace(_expression))
            {
                throw new ModelValidationException("Param location or expression must be specified.");
            }
            else if (!string.IsNullOrWhiteSpace(_location) && !string.IsNullOrWhiteSpace(_expression))
            {
                throw new ModelValidationException("Only one of param location and expression can be specified.");
            }
            else if (!string.IsNullOrWhiteSpace(_location))
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
