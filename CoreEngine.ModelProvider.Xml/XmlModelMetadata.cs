using CoreEngine.Abstractions.Model;
using CoreEngine.Abstractions.Model.States.Metadata;
using CoreEngine.ModelProvider.Xml.States;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml
{
    public class XmlModelMetadata : IModelMetadata
    {
        private readonly XDocument _document;

        public XmlModelMetadata(XDocument document)
        {
            _document = document;
        }

        public Task<IRootStateMetadata> GetRootState()
        {
            return Task.FromResult((IRootStateMetadata) new RootStateMetadata(_document.Root));
        }
    }
}
