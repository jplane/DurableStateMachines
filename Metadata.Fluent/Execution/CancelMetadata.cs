using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class CancelMetadata<TParent> : ExecutableContentMetadata, ICancelMetadata where TParent : IModelMetadata
    {
        private string _sendId;
        private string _sendIdExpr;

        internal CancelMetadata()
        {
        }

        internal TParent Parent { get; set; }

        public CancelMetadata<TParent> SendId(string sendId)
        {
            _sendId = sendId;
            return this;
        }

        public CancelMetadata<TParent> SendIdExpression(string sendIdExpr)
        {
            _sendIdExpr = sendIdExpr;
            return this;
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        string ICancelMetadata.SendId => _sendId;

        string ICancelMetadata.SendIdExpr => _sendIdExpr;
    }
}
