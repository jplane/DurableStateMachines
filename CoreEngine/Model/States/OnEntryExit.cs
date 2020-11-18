using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.Model.Execution;
using System;

namespace StateChartsDotNet.CoreEngine.Model.States
{
    internal class OnEntryExit
    {
        private readonly Lazy<ExecutableContent[]> _content;
        private readonly bool _isEntry;

        public OnEntryExit(IOnEntryExitMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _isEntry = metadata.IsEntry;

            _content = new Lazy<ExecutableContent[]>(() =>
            {
                return metadata.GetExecutableContent().Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var name = _isEntry ? "OnEntry" : "OnExit";

            context.LogInformation($"Start: {name}");

            try
            {
                foreach (var content in _content.Value)
                {
                    await content.Execute(context);
                }
            }
            finally
            {
                context.LogInformation($"End: {name}");
            }
        }
    }
}
