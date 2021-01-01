using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Xml.Data;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.States
{
    public class InvokeStateChartMetadata : IInvokeStateChartMetadata
    {
        private readonly XElement _element;
        private readonly string _metadataId;

        internal InvokeStateChartMetadata(XElement element)
        {
            _element = element;
            _metadataId = element.GetUniqueElementPath();
        }

        public string MetadataId => _metadataId;

        public bool Autoforward => bool.Parse(_element.Attribute("autoforward")?.Value ?? "false");

        public ChildStateChartExecutionMode ExecutionMode
        {
            get
            {
                var attr = _element.Attribute("mode");

                if (attr != null && Enum.TryParse(attr.Value, out ChildStateChartExecutionMode result))
                {
                    return result;
                }
                else
                {
                    return ChildStateChartExecutionMode.Isolated;
                }
            }
        }

        public string RemoteUri => _element.Attribute("remoteuri")?.Value ?? string.Empty;

        public virtual bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
        }

        public string Id => _element.Attribute("id")?.Value ?? string.Empty;

        public string IdLocation => _element.Attribute("idlocation")?.Value ?? string.Empty;

        private IEnumerable<string> Namelist
        {
            get
            {
                var names = _element?.Attribute("namelist")?.Value;

                if (string.IsNullOrWhiteSpace(names))
                {
                    return Enumerable.Empty<string>();
                }
                else
                {
                    return names.Split(" ");
                }
            }
        }

        public IEnumerable<IExecutableContentMetadata> GetFinalizeExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            var elements = _element.ScxmlElement("finalize")?.Elements() ?? Enumerable.Empty<XElement>();

            foreach (var node in elements)
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return content.AsEnumerable();
        }

        public IStateChartMetadata GetRoot()
        {
            var node = _element.ScxmlElement("content");

            if (node != null)
            {
                if (node.ScxmlElement("scxml") != null)
                {
                    var xdoc = XDocument.Parse(node.ScxmlElement("scxml").ToString());

                    Debug.Assert(xdoc != null);

                    return new StateChart(xdoc);
                }
            }

            return null;
        }

        public IReadOnlyDictionary<string, object> GetParams(dynamic data)
        {
            var nodes = _element.ScxmlElements("param");

            if (this.Namelist.Any() && nodes.Any())
            {
                throw new MetadataValidationException("Only one of service namelist and <params> can be specified.");
            }

            IEnumerable<ParamMetadata> parms;

            if (this.Namelist.Any())
            {
                parms = this.Namelist.Select(n => new ParamMetadata(n));
            }
            else
            {
                parms = nodes.Select(n => new ParamMetadata(n));
            }

            return new ReadOnlyDictionary<string, object>(parms.ToDictionary(p => p.Name, p => p.GetValue(data)));
        }
    }
}
