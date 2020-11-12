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

            var foreachMetadata = (IForeachMetadata) _metadata;

            var enumerable = await foreachMetadata.GetArray(context.ScriptData);

            if (enumerable == null)
            {
                context.LogDebug($"Foreach: Array is null");
                return;
            }

            var shallowCopy = enumerable.OfType<object>().ToArray();

            context.LogDebug($"Foreach: Array length {shallowCopy.Length}");

            Debug.Assert(foreachMetadata.Item != null);

            for (var idx = 0; idx < shallowCopy.Length; idx++)
            {
                var item = shallowCopy[idx];

                context.SetDataValue(foreachMetadata.Item, item);

                if (!string.IsNullOrWhiteSpace(foreachMetadata.Index))
                {
                    context.SetDataValue(foreachMetadata.Index, idx);

                    context.LogDebug($"Foreach: Array item index {foreachMetadata.Index}");
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
                    context.SetDataValue(foreachMetadata.Item, null);

                    if (!string.IsNullOrWhiteSpace(foreachMetadata.Index))
                    {
                        context.SetDataValue(foreachMetadata.Index, null);
                    }
                }
            }
        }
    }
}
