using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Dynamic;
using System.Xml.Linq;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution
{
    public sealed class LogMetadata<TParent> : ExecutableContentMetadata, ILogMetadata where TParent : IModelMetadata
    {
        private Func<dynamic, string> _getMessage;

        internal LogMetadata()
        {
        }

        internal TParent Parent { get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public LogMetadata<TParent> Message(Func<dynamic, string> getter)
        {
            _getMessage = getter;
            return this;
        }

        string ILogMetadata.GetMessage(dynamic data) => _getMessage?.Invoke(data) ?? string.Empty;
    }
}
