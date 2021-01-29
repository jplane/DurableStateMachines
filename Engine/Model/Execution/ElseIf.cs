using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System;

namespace StateChartsDotNet.Model.Execution
{
    internal class ElseIf<TData>
    {
        private readonly Lazy<ExecutableContent<TData>[]> _content;
        private readonly IElseIfMetadata _metadata;

        public ElseIf(IElseIfMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;

            _content = new Lazy<ExecutableContent<TData>[]>(() =>
            {
                return _metadata.GetExecutableContent().Select(ExecutableContent<TData>.Create).ToArray();
            });
        }

        public async Task<bool> ConditionalExecuteAsync(ExecutionContextBase<TData> context)
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
