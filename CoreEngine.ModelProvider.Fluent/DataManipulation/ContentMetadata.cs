using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.DataManipulation
{
    public sealed class ContentMetadata<TParent> : IContentMetadata where TParent : IModelMetadata
    {
        private Func<dynamic, object> _payloadGetter;

        internal ContentMetadata()
        {
        }

        public ContentMetadata<TParent> WithPayload(Func<dynamic, object> getter)
        {
            _payloadGetter = getter;
            return this;
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        internal TParent Parent { get; set; }

        internal string UniqueId { private get; set; }

        string IModelMetadata.UniqueId => this.UniqueId;

        string IContentMetadata.Expression => throw new NotImplementedException();

        string IContentMetadata.Body => throw new NotImplementedException();

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new NotImplementedException();
        }
    }
}
