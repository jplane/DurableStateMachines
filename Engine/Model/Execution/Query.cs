using System;
using System.Linq;
using StateChartsDotNet.Common;
using System.Threading.Tasks;
using StateChartsDotNet.Common.Model.Execution;
using System.Diagnostics;

namespace StateChartsDotNet.Model.Execution
{
    internal class Query : ExecutableContent
    {
        private readonly Lazy<ExecutableContent[]> _content;

        public Query(IQueryMetadata metadata)
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

            var metadata = (IQueryMetadata) _metadata;

            var type = metadata.GetType(context.ScriptData);

            Debug.Assert(!string.IsNullOrWhiteSpace(type));

            var target = metadata.GetTarget(context.ScriptData);

            Debug.Assert(!string.IsNullOrWhiteSpace(target));

            var parms = metadata.GetParams(context.ScriptData);

            Debug.Assert(parms != null);

            try
            {
                var result = await context.QueryAsync(type, target, parms);

                context.SetDataValue(metadata.ResultLocation, result);

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
