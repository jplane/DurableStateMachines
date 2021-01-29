using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections;

namespace StateChartsDotNet.Model.Execution
{
    internal class Foreach<TData> : ExecutableContent<TData>
    {
        private readonly Lazy<ExecutableContent<TData>[]> _content;

        public Foreach(IForeachMetadata metadata)
            : base(metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new Lazy<ExecutableContent<TData>[]>(() =>
            {
                return metadata.GetExecutableContent().Select(ExecutableContent<TData>.Create).ToArray();
            });
        }

        protected override async Task _ExecuteAsync(ExecutionContextBase<TData> context)
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

            Debug.Assert(! string.IsNullOrWhiteSpace(foreachMetadata.Item));

            for (var idx = 0; idx < items.Length; idx++)
            {
                await context.LogDebugAsync($"Foreach: Array item index {idx}");

                var item = items[idx];

                context.SetDataValue(foreachMetadata.Item, item);

                if (!string.IsNullOrWhiteSpace(foreachMetadata.Index))
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
