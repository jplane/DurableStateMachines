using System;
using System.Linq;
using DSM.Common;
using System.Threading.Tasks;
using DSM.Common.Model.Actions;
using DSM.Engine;

namespace DSM.Engine.Model.Actions
{
    internal class Query : Action
    {
        private readonly Lazy<Action[]> _content;

        public Query(IQueryMetadata metadata)
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

            var metadata = (IQueryMetadata) _metadata;

            try
            {
                var result = await context.QueryAsync(metadata.ActivityType, metadata.GetConfiguration());

                if (!metadata.ResultLocation.Equals(default))
                {
                    context.SetDataValue(metadata.ResultLocation, result);
                }

                foreach (var content in _content.Value)
                {
                    await content.ExecuteAsync(context);
                }
            }
            catch (TaskCanceledException)
            {
                context.InternalCancel();
            }
            catch (Exception ex)
            {
                context.EnqueueCommunicationError(ex);
            }
        }
    }
}
