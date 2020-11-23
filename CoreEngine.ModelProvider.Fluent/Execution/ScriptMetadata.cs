using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution
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
