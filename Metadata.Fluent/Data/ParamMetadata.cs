using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Metadata.Fluent.Data
{
    public sealed class ParamMetadata<TParent> where TParent : IModelMetadata
    {
        private readonly string _name;
        private Func<dynamic, object> _valueGetter;

        internal ParamMetadata(string name)
        {
            _name = name;
        }

        internal TParent Parent { get; set; }

        internal string MetadataId { private get; set; }

        public ParamMetadata<TParent> Value(Func<dynamic, object> getter)
        {
            _valueGetter = getter;
            return this;
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        internal bool Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new System.NotImplementedException();
        }

        internal string Name => _name;

        internal object GetValue(dynamic data) => _valueGetter?.Invoke(data);
    }
}
