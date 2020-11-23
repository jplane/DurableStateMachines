using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class AssignMetadata<TParent> : ExecutableContentMetadata, IAssignMetadata where TParent : IModelMetadata
    {
        private string _location;
        private Func<dynamic, object> _getValue;

        internal AssignMetadata()
        {
        }

        internal TParent Parent { get; set; }

        public AssignMetadata<TParent> Location(string location)
        {
            _location = location;
            return this;
        }

        public AssignMetadata<TParent> Value(Func<dynamic, object> getter)
        {
            _getValue = getter;
            return this;
        }

        public TParent Attach()
        {
            return this.Parent;
        }

        string IAssignMetadata.Location => _location;

        object IAssignMetadata.GetValue(dynamic data) => _getValue;
    }
}
