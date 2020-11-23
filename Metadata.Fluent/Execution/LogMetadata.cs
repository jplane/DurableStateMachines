using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;

namespace StateChartsDotNet.Metadata.Fluent.Execution
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
