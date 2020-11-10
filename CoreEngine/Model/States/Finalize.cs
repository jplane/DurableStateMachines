using System.Linq;
using CoreEngine.Model.Execution;
using Nito.AsyncEx;
using CoreEngine.Abstractions.Model.States.Metadata;

namespace CoreEngine.Model.States
{
    internal class Finalize
    {
        private readonly AsyncLazy<ExecutableContent[]> _content;

        public Finalize(IFinalizeMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new AsyncLazy<ExecutableContent[]>(async () =>
            {
                return (await metadata.GetExecutableContent()).Select(ExecutableContent.Create).ToArray();
            });
        }
    }
}
