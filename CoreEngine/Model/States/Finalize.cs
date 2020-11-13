using System.Linq;
using StateChartsDotNet.CoreEngine.Model.Execution;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;

namespace StateChartsDotNet.CoreEngine.Model.States
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
