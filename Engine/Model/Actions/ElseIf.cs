using System.Linq;
using System.Threading.Tasks;
using DSM.Common;
using DSM.Common.Model.Actions;
using System;
using DSM.Engine;

namespace DSM.Engine.Model.Actions
{
    internal class ElseIf
    {
        private readonly Lazy<Action[]> _content;
        private readonly IElseIfMetadata _metadata;

        public ElseIf(IElseIfMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;

            _content = new Lazy<Action[]>(() =>
            {
                return _metadata.GetActions().Select(Action.Create).ToArray();
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
