using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution
{
    public sealed class RaiseMetadata<TParent> : ExecutableContentMetadata, IRaiseMetadata where TParent : IModelMetadata
    {
        private string _messageName;

        internal RaiseMetadata()
        {
        }

        internal TParent Parent { get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public RaiseMetadata<TParent> WithMessageName(string messageName)
        {
            _messageName = messageName;
            return this;
        }

        string IRaiseMetadata.MessageName => _messageName;
    }
}
