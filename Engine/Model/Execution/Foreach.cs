using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using DSM.Common;
using DSM.Common.Model.Execution;
using System;
using System.Collections;
using DSM.Engine;

namespace DSM.Engine.Model.Execution
{
    internal class Foreach : ExecutableContent
    {
        private readonly Lazy<ExecutableContent[]> _content;

        public Foreach(IForeachMetadata metadata)
            : base(metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new Lazy<ExecutableContent[]>(() =>
            {
                return metadata.GetExecutableContent().Select(ExecutableContent.Create).ToArray();
            });
        }

        protected override async Task _ExecuteAsync(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            var foreachMetadata = (IForeachMetadata) _metadata;

            var enumerable = foreachMetadata.GetArray(context.ExecutionData);

            if (enumerable == null)
            {
                await context.LogDebugAsync($"Foreach: Array is null");
                return;
            }

            var items = enumerable.Cast<object>().ToArray();

            await context.LogDebugAsync($"Foreach: Array length {items.Length}");

            for (var idx = 0; idx < items.Length; idx++)
            {
                await context.LogDebugAsync($"Foreach: Array item index {idx}");

                var item = items[idx];

                if (!foreachMetadata.Item.Equals(default))
                {
                    context.SetDataValue(foreachMetadata.Item, item);
                }

                if (!foreachMetadata.Index.Equals(default))
                {
                    context.SetDataValue(foreachMetadata.Index, idx);
                }

                foreach (var content in _content.Value)
                {
                    await content.ExecuteAsync(context);
                }
            }
        }
    }
}
