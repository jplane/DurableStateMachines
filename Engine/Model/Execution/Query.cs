using System;
using System.Linq;
using StateChartsDotNet.Common;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.Execution;
using System.Diagnostics;

namespace StateChartsDotNet.Model.Execution
{
    internal class Query<TData> : ExecutableContent<TData>
    {
        private readonly Lazy<ExecutableContent<TData>[]> _content;

        public Query(IQueryMetadata metadata)
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

            var metadata = (IQueryMetadata) _metadata;

            try
            {
                var result = await context.QueryAsync(metadata.ActivityType, metadata.Configuration);

                if (!string.IsNullOrWhiteSpace(metadata.ResultLocation))
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
