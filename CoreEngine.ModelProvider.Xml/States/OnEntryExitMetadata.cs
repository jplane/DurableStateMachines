using CoreEngine.Abstractions.Model.Execution.Metadata;
using CoreEngine.Abstractions.Model.States.Metadata;
using CoreEngine.ModelProvider.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.States
{
    public class OnEntryExitMetadata : IOnEntryExitMetadata
    {
        private readonly XElement _element;

        public OnEntryExitMetadata(XElement element)
        {
            _element = element;
        }

        public bool IsEntry => _element.Name.LocalName.ToLowerInvariant() == "onentry";

        public Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent()
        {
            var content = new List<IExecutableContentMetadata>();

            foreach (var node in _element.Elements())
            {
                content.Add(ExecutableContentMetadata.Create(node));
            }

            return Task.FromResult(content.AsEnumerable());
        }
    }
}
