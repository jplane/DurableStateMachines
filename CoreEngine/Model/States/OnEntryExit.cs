using System.Linq;
using System.Threading.Tasks;
using CoreEngine.Abstractions.Model.States.Metadata;
using CoreEngine.Model.Execution;
using Nito.AsyncEx;

namespace CoreEngine.Model.States
{
    internal class OnEntryExit
    {
        private readonly AsyncLazy<ExecutableContent[]> _content;
        private readonly bool _isEntry;

        public OnEntryExit(IOnEntryExitMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _isEntry = metadata.IsEntry;

            _content = new AsyncLazy<ExecutableContent[]>(async () =>
            {
                return (await metadata.GetExecutableContent()).Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var name = _isEntry ? "OnEntry" : "OnExit";

            context.LogInformation($"Start: {name}");

            try
            {
                foreach (var content in await _content)
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
