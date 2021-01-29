using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System.Diagnostics;
using System;

namespace StateChartsDotNet.Model.Execution
{
    internal class If<TData> : ExecutableContent<TData>
    {
        private readonly Lazy<ElseIf<TData>[]> _elseifs;
        private readonly Lazy<Else<TData>> _else;
        private readonly Lazy<ExecutableContent<TData>[]> _content;

        public If(IIfMetadata metadata)
            : base(metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new Lazy<ExecutableContent<TData>[]>(() =>
            {
                return metadata.GetExecutableContent().Select(ExecutableContent<TData>.Create).ToArray();
            });

            _else = new Lazy<Else<TData>>(() =>
            {
                var elseMetadata = metadata.GetElse();

                if (elseMetadata != null)
                {
                    return new Else<TData>(elseMetadata);
                }
                else
                {
                    return null;
                }
            });

            _elseifs = new Lazy<ElseIf<TData>[]>(() =>
                metadata.GetElseIfs().Select(metadata => new ElseIf<TData>(metadata)).ToArray());
        }

        protected override async Task _ExecuteAsync(ExecutionContextBase<TData> context)
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
