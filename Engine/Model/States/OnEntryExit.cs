using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Model.Execution;
using System;

namespace StateChartsDotNet.Model.States
{
    internal class OnEntryExit<TData>
    {
        private readonly Lazy<ExecutableContent<TData>[]> _content;
        private readonly bool _isEntry;

        public OnEntryExit(IOnEntryExitMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _isEntry = metadata.IsEntry;

            _content = new Lazy<ExecutableContent<TData>[]>(() =>
            {
                return metadata.GetExecutableContent().Select(ExecutableContent<TData>.Create).ToArray();
            });
        }

        public async Task ExecuteAsync(ExecutionContextBase<TData> context)
        {
            context.CheckArgNull(nameof(context));

            var name = _isEntry ? "OnEntry" : "OnExit";

            await context.LogInformationAsync($"Start: {name}");

            try
            {
                foreach (var content in _content.Value)
                {
                    await content.ExecuteAsync(context);
                }
            }
            finally
            {
                await context.LogInformationAsync($"End: {name}");
            }
        }
    }
}
