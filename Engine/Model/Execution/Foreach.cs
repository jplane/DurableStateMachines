using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.Execution;
using System;

namespace StateChartsDotNet.Model.Execution
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

        protected override async Task _ExecuteAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var foreachMetadata = (IForeachMetadata) _metadata;

            var enumerable = foreachMetadata.GetArray(context.ScriptData);

            if (enumerable == null)
            {
                await context.LogDebugAsync($"Foreach: Array is null");
                return;
            }

            var shallowCopy = enumerable.OfType<object>().ToArray();

            await context.LogDebugAsync($"Foreach: Array length {shallowCopy.Length}");

            Debug.Assert(foreachMetadata.Item != null);

            for (var idx = 0; idx < shallowCopy.Length; idx++)
            {
                var item = shallowCopy[idx];

                context.SetDataValue(foreachMetadata.Item, item);

                if (!string.IsNullOrWhiteSpace(foreachMetadata.Index))
                {
                    context.SetDataValue(foreachMetadata.Index, idx);

                    await context.LogDebugAsync($"Foreach: Array item index {foreachMetadata.Index}");
                }

                try
                {
                    foreach (var content in _content.Value)
                    {
                        await content.ExecuteAsync(context);
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
