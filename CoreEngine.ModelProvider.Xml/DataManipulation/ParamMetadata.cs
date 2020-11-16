using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Xml.DataManipulation
{
    public class ParamMetadata : IParamMetadata
    {
        private readonly string _location;
        private readonly string _expression;
        private readonly Lazy<Func<dynamic, object>> _getExpressionValue;

        public ParamMetadata(XElement element)
        {
            this.Name = element.Attribute("name").Value;
            _location = element.Attribute("location")?.Value ?? string.Empty;
            _expression = element.Attribute("expr")?.Value ?? string.Empty;

            _getExpressionValue = new Lazy<Func<dynamic, object>>(() =>
            {
                return ExpressionCompiler.Compile<object>(_expression);
            });
        }

        public ParamMetadata(string location)
        {
            this.Name = location;
            _location = location;
            _expression = null;
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
