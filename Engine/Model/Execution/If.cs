using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System.Diagnostics;
using System;

namespace StateChartsDotNet.Model.Execution
{
    internal class If : ExecutableContent
    {
        private readonly Lazy<ElseIf[]> _elseifs;
        private readonly Lazy<Else> _else;
        private readonly Lazy<ExecutableContent[]> _content;

        public If(IIfMetadata metadata)
            : base(metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new Lazy<ExecutableContent[]>(() =>
            {
                return metadata.GetExecutableContent().Select(ExecutableContent.Create).ToArray();
            });

            _else = new Lazy<Else>(() =>
            {
                var elseMetadata = metadata.GetElse();

                if (elseMetadata != null)
                {
                    return new Else(elseMetadata);
                }
                else
                {
                    return null;
                }
            });

            _elseifs = new Lazy<ElseIf[]>(() =>
                metadata.GetElseIfs().Select(metadata => new ElseIf(metadata)).ToArray());
        }

        protected override async Task _ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            var result = ((IIfMetadata) _metadata).EvalCondition(context.ExecutionData);

            await context.LogDebugAsync($"Condition = {result}");

            if (result)
            {
                foreach (var content in _content.Value)
                {
                    await content.ExecuteAsync(context);
                }
            }
            else
            {
                foreach (var elseif in _elseifs.Value)
                {
                    if (await elseif.ConditionalExecuteAsync(context))
                    {
                        return;
                    }
                }

                if (_else.Value != null)
                {
                    await _else.Value.Execute(context);
                }
            }
        }
    }
}
