using System.Linq;
using System.Threading.Tasks;
using DSM.Common;
using DSM.Common.Model.Execution;
using System;
using DSM.Engine;

namespace DSM.Engine.Model.Execution
{
    internal class ElseIf
    {
        private readonly Lazy<ExecutableContent[]> _content;
        private readonly IElseIfMetadata _metadata;

        public ElseIf(IElseIfMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;

            _content = new Lazy<ExecutableContent[]>(() =>
            {
                return _metadata.GetExecutableContent().Select(ExecutableContent.Create).ToArray();
            });
        }

        public async Task<bool> ConditionalExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            await context.LogInformationAsync("Start: ElseIf.Execute");

            try
            {
                var result = _metadata.EvalCondition(context.ExecutionData);

                await context.LogDebugAsync($"Condition = {result}");

                if (result)
                {
                    foreach (var content in _content.Value)
                    {
                        await content.ExecuteAsync(context);
                    }
                }

                return result;
            }
            finally
            {
                await context.LogInformationAsync("End: ElseIf.Execute");
            }
        }
    }
}
