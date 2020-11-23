using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class ScriptMetadata<TParent> : ExecutableContentMetadata, IScriptMetadata where TParent : IModelMetadata
    {
        private Action<dynamic> _action;

        internal ScriptMetadata()
        {
        }

        internal TParent Parent { get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public ScriptMetadata<TParent> Action(Action<dynamic> action)
        {
            _action = action;
            return this;
        }

        void IScriptMetadata.Execute(dynamic data) => _action?.Invoke(data);
    }
}
