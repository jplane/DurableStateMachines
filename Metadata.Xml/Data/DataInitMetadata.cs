using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.Data
{
    public class DataInitMetadata : IDataInitMetadata
    {
        private readonly XElement _element;
        private readonly Lazy<Func<dynamic, object>> _getter;
        private readonly string _metadataId;

        internal DataInitMetadata(XElement element)
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

            _metadataId = element.GetUniqueElementPath();
        }

        public string MetadataId => _metadataId;

        public string Id => _element.Attribute("id").Value;

        public object GetValue(dynamic data)
        {
            return _getter.Value(data);
        }

        private string Expression => _element.Attribute("expr")?.Value ?? string.Empty;

        private string Body => _element.Value ?? string.Empty;
    }
}
