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

        protected override async Task _ExecuteAsync(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            var metadata = (IQueryMetadata) _metadata;

            await context.ExecuteContentAsync(metadata.UniqueId, async ec =>
            {
                Debug.Assert(ec != null);

                try
                {
                    var type = metadata.GetType(ec.ScriptData);

                    if (string.IsNullOrWhiteSpace(type))
                    {
                        throw new InvalidOperationException("External query type not specified.");
                    }

                    var query = ec.GetExternalQuery(type);

                    if (query == null)
                    {
                        throw new InvalidOperationException($"External query '{type}' not configured.");
                    }

                    var target = metadata.GetTarget(ec.ScriptData);

                    var parms = metadata.GetParams(ec.ScriptData);

                    var result = await query(target, parms);

                    ec.SetDataValue(metadata.ResultLocation, result);
                }
                catch(Exception ex)
                {
                    ec.EnqueueCommunicationError(ex);
                }
            });

            foreach (var content in _content.Value)
            {
                await content.ExecuteAsync(context);
            }
        }
    }
}
