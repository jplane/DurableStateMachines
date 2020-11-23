using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.DataManipulation;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Xml.DataManipulation;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.States
{
    public class InvokeStateChart : IInvokeStateChartMetadata
    {
        private readonly XElement _element;
        private readonly Lazy<string> _uniqueId;

        public InvokeStateChart(XElement element)
        {
            _element = element;

            _uniqueId = new Lazy<string>(() =>
            {
                return element.GetUniqueElementPath();
            });
        }

        public bool Autoforward
        {
            get
            {
                var afattr = _element.Attribute("autoforward");

                if (afattr != null && bool.TryParse(afattr.Value, out bool result))
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
        }

        public string UniqueId => _uniqueId.Value;

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

        public IContentMetadata GetContent()
        {
            var node = _element.ScxmlElement("content");

            return node == null ? null : (IContentMetadata) new ContentMetadata(node);
        }

        public IEnumerable<IExecutableContentMetadata> GetFinalizeExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            foreach (var node in _element.Elements())
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return content.AsEnumerable();
        }

        public IEnumerable<IParamMetadata> GetParams()
        {
            var nodes = _element.ScxmlElements("param");

            if (!this.Namelist.Any() && !nodes.Any())
            {
                throw new ModelValidationException("Service namelist or <params> must be specified.");
            }
            else if (this.Namelist.Any() && nodes.Any())
            {
                throw new ModelValidationException("Only one of service namelist and <params> can be specified.");
            }
            else if (this.Namelist.Any())
            {
                return this.Namelist.Select(n => new ParamMetadata(n)).Cast<IParamMetadata>();
            }
            else
            {
                return nodes.Select(n => new ParamMetadata(n)).Cast<IParamMetadata>();
            }
        }
    }
}
