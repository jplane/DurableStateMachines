using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;

namespace StateChartsDotNet.Metadata.Fluent.Execution
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

        public RaiseMetadata<TParent> MessageName(string messageName)
        {
            _messageName = messageName;
            return this;
        }

        string IRaiseMetadata.MessageName => _messageName;
    }
}
