using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace StateChartsDotNet.Metadata.Xml.States
{
    public class OnEntryExitMetadata : IOnEntryExitMetadata
    {
        private readonly XElement _element;
        private readonly Lazy<string> _uniqueId;

        internal OnEntryExitMetadata(XElement element)
        {
            _element = element;

            _uniqueId = new Lazy<string>(() =>
            {
                return element.GetUniqueElementPath();
            });
        }

        public string UniqueId => _uniqueId.Value;

        public virtual bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            return true;
        }

        public bool IsEntry => _element.Name.LocalName.ToLowerInvariant() == "onentry";

        public IEnumerable<IExecutableContentMetadata> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            foreach (var node in _element.Elements())
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return content;
        }
    }
}
