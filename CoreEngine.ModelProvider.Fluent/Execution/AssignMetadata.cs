using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution
{
    public sealed class AssignMetadata<TParent> : ExecutableContentMetadata, IAssignMetadata where TParent : IModelMetadata
    {
        private string _location;
        private Func<dynamic, object> _getValue;

        internal AssignMetadata()
        {
        }

        internal TParent Parent { get; set; }

        public AssignMetadata<TParent> WithLocation(string location)
        {
            _location = location;
            return this;
        }

        public AssignMetadata<TParent> WithValue(Func<dynamic, object> getter)
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
