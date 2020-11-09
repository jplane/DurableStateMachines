using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using CoreEngine.Abstractions.Model.Execution.Metadata;
using CoreEngine.Abstractions.Model.States.Metadata;
using CoreEngine.ModelProvider.Xml.DataManipulation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.ModelProvider.Xml.States
{
    public class FinalStateMetadata : StateMetadata, IFinalStateMetadata
    {
        public FinalStateMetadata(XElement element)
            : base(element)
        {
        }

        public Task<IDonedataMetadata> GetDonedata()
        {
            var node = _element.ScxmlElement("donedata");

            return Task.FromResult(node == null ? null : (IDonedataMetadata) new DonedataMetadata(node));
        }
    }
}
