using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Debugger;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.Execution
{
    internal abstract class ExecutableContent<TData>
    {
        protected readonly IExecutableContentMetadata _metadata;

        protected ExecutableContent(IExecutableContentMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
        }

        public static ExecutableContent<TData> Create(IExecutableContentMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            ExecutableContent<TData> content = null;

            switch (metadata)
            {
                case IIfMetadata @if:
                    content = new If<TData>(@if);
                    break;
                case IRaiseMetadata raise:
                    content = new Raise<TData>(raise);
                    break;
                case IScriptMetadata script:
                    content = new Script<TData>(script);
                    break;
                case IForeachMetadata @foreach:
                    content = new Foreach<TData>(@foreach);
                    break;
                case ILogMetadata log:
                    content = new Log<TData>(log);
                    break;
                case ISendMessageMetadata send:
                    content = new SendMessage<TData>(send);
                    break;
                case IAssignMetadata assign:
                    content = new Assign<TData>(assign);
                    break;
                case IQueryMetadata query:
                    content = new Query<TData>(query);
                    break;
            }

            Debug.Assert(content != null, $"Executable Metadata is not a recognized type: {metadata.GetType().FullName}");

            return content;
        }

        protected abstract Task _ExecuteAsync(ExecutionContextBase<TData> context);

        public async Task ExecuteAsync(ExecutionContextBase<TData> context)
        {
            await context.LogInformationAsync($"Start: {this.GetType().Name}.Execute");

            await context.BreakOnDebugger(DebuggerAction.BeforeAction, _metadata);

            try
            {
                await _ExecuteAsync(context);
            }
            catch (StateChartException)
            {
                throw;
            }
            catch (Exception ex)
            {
                context.EnqueueExecutionError(ex);
            }
            finally
            {
                await context.BreakOnDebugger(DebuggerAction.AfterAction, _metadata);

                await context.LogInformationAsync($"End: {this.GetType().Name}.Execute");
            }
        }
    }
}
