using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution
{
    public sealed class CancelMetadata<TParent> : ExecutableContentMetadata, ICancelMetadata where TParent : IModelMetadata
    {
        private string _sendId;
        private string _sendIdExpr;

        internal CancelMetadata()
        {
        }

        internal TParent Parent { get; set; }

        public CancelMetadata<TParent> WithSendId(string sendId)
        {
            _sendId = sendId;
            return this;
        }

        public CancelMetadata<TParent> WithSendIdExpression(string sendIdExpr)
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
