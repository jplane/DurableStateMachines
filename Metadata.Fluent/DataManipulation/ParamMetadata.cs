using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.DataManipulation;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Metadata.Fluent.DataManipulation
{
    public sealed class ParamMetadata<TParent> : IParamMetadata where TParent : IModelMetadata
    {
        private string _name;
        private Func<dynamic, object> _valueGetter;

        internal ParamMetadata()
        {
        }

        internal TParent Parent { get; set; }

        internal string UniqueId { private get; set; }

        public ParamMetadata<TParent> Name(string name)
        {
            _name = name;
            return this;
        }

        public ParamMetadata<TParent> Value(Func<dynamic, object> getter)
        {
            _valueGetter = getter;
            return this;
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        string IModelMetadata.UniqueId => this.UniqueId;

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new System.NotImplementedException();
        }

        string IParamMetadata.Name => _name;

        object IParamMetadata.GetValue(dynamic data) => _valueGetter?.Invoke(data);
    }
}
