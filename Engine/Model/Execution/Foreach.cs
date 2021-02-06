using System.Linq;
using System.Threading.Tasks;
using DSM.Common;
using DSM.Common.Model.Execution;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DSM.Engine.Model.Execution
{
    internal class Foreach : Action
    {
        private readonly Lazy<Action[]> _content;

        public Foreach(IForeachMetadata metadata)
            : base(metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new Lazy<Action[]>(() =>
            {
                return metadata.GetActions().Select(Action.Create).ToArray();
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

                object item = items[idx];

                if (item is JToken jt)
                {
                    item = JsonConvert.DeserializeObject(jt.ToString());
                }

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
