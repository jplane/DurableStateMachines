using System.Linq;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using Nito.AsyncEx;
using CoreEngine.Abstractions.Model.Execution.Metadata;

namespace CoreEngine.Model.Execution
{
    internal class Foreach : ExecutableContent
    {
        private readonly AsyncLazy<ExecutableContent[]> _content;

        public Foreach(IForeachMetadata metadata)
            : base(metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new AsyncLazy<ExecutableContent[]>(async () =>
            {
                return (await metadata.GetExecutableContent()).Select(ExecutableContent.Create).ToArray();
            });
        }

        protected override async Task _Execute(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var enumerable = await context.Eval<IEnumerable>(((IForeachMetadata) _metadata).ArrayExpression);

            if (enumerable == null)
            {
                context.LogDebug($"Foreach: Array is null");
                return;
            }

            var shallowCopy = enumerable.OfType<object>().ToArray();

            context.LogDebug($"Foreach: Array length {shallowCopy.Length}");

            Debug.Assert(((IForeachMetadata) _metadata).Item != null);

            for (var idx = 0; idx < shallowCopy.Length; idx++)
            {
                var item = shallowCopy[idx];

                context.SetDataValue(((IForeachMetadata) _metadata).Item, item);

                if (!string.IsNullOrWhiteSpace(((IForeachMetadata) _metadata).Index))
                {
                    context.SetDataValue(((IForeachMetadata) _metadata).Index, idx);

                    context.LogDebug($"Foreach: Array item index {((IForeachMetadata) _metadata).Index}");
                }

                try
                {
                    foreach (var content in await _content)
                    {
                        await content.Execute(context);
                    }
                }
                finally
                {
                    context.SetDataValue(((IForeachMetadata) _metadata).Item, null);

                    if (!string.IsNullOrWhiteSpace(((IForeachMetadata) _metadata).Index))
                    {
                        context.SetDataValue(((IForeachMetadata) _metadata).Index, null);
                    }
                }
            }
        }
    }
}
