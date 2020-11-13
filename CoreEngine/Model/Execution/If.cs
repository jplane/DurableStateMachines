﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System.Diagnostics;

namespace StateChartsDotNet.CoreEngine.Model.Execution
{
    internal class If : ExecutableContent
    {
        private readonly AsyncLazy<ElseIf[]> _elseifs;
        private readonly AsyncLazy<Else> _else;
        private readonly AsyncLazy<ExecutableContent[]> _content;

        public If(IIfMetadata metadata)
            : base(metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new AsyncLazy<ExecutableContent[]>(async () =>
            {
                return (await metadata.GetExecutableContent()).Select(ExecutableContent.Create).ToArray();
            });

            _else = new AsyncLazy<Else>(async () =>
            {
                var elseExecutableContent = await metadata.GetElseExecutableContent();

                if (elseExecutableContent != null)
                {
                    return new Else(elseExecutableContent);
                }
                else
                {
                    return null;
                }
            });

            _elseifs = new AsyncLazy<ElseIf[]>(async () =>
            {
                var elseifs = new List<ElseIf>();

                var conditions = (await metadata.GetElseIfConditions()).ToArray();

                var content = metadata.GetElseIfExecutableContent().ToArray();

                Debug.Assert(conditions.Length == content.Length);

                for (var i = 0; i < conditions.Length; i++)
                {
                    elseifs.Add(new ElseIf(conditions[i], content[i]));
                }

                return elseifs.ToArray();
            });
        }

        protected override async Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var result = await ((IIfMetadata) _metadata).EvalIfCondition(context.ScriptData);

            context.LogDebug($"Condition = {result}");

            if (result)
            {
                foreach (var content in await _content)
                {
                    await content.Execute(context);
                }
            }
            else
            {
                foreach (var elseif in await _elseifs)
                {
                    if (await elseif.ConditionalExecute(context))
                    {
                        return;
                    }
                }

                if ((await _else) != null)
                {
                    await (await _else)?.Execute(context);
                }
            }
        }
    }
}
