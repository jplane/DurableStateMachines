using System.Linq;
using System.Threading.Tasks;
using DSM.Common;
using DSM.Common.Model.States;
using DSM.Engine.Model.Execution;
using System;
using DSM.Engine;

namespace DSM.Engine.Model.States
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

        public async Task ExecuteAsync(ExecutionContextBase context)
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
