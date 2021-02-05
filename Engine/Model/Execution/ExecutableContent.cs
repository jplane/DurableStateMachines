using DSM.Common;
using DSM.Common.Exceptions;
using DSM.Common.Model.Execution;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DSM.Engine;
using DSM.Common.Observability;

namespace DSM.Engine.Model.Execution
{
    internal abstract class ExecutableContent
    {
        protected readonly IExecutableContentMetadata _metadata;

        protected ExecutableContent(IExecutableContentMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
        }

        public static ExecutableContent Create(IExecutableContentMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            ExecutableContent content = null;

            switch (metadata)
            {
                case IIfMetadata @if:
                    content = new If(@if);
                    break;
                case IRaiseMetadata raise:
                    content = new Raise(raise);
                    break;
                case ILogicMetadata script:
                    content = new Script(script);
                    break;
                case IForeachMetadata @foreach:
                    content = new Foreach(@foreach);
                    break;
                case ILogMetadata log:
                    content = new Log(log);
                    break;
                case ISendMessageMetadata send:
                    content = new SendMessage(send);
                    break;
                case IAssignMetadata assign:
                    content = new Assign(assign);
                    break;
                case IQueryMetadata query:
                    content = new Query(query);
                    break;
            }

            Debug.Assert(content != null, $"Executable Metadata is not a recognized type: {metadata.GetType().FullName}");

            return content;
        }

        protected abstract Task _ExecuteAsync(ExecutionContextBase context);

        public async Task ExecuteAsync(ExecutionContextBase context)
        {
            await context.LogInformationAsync($"Start: {this.GetType().Name}.Execute");

            await context.OnAction(ObservableAction.BeforeAction, _metadata);

            try
            {
                await _ExecuteAsync(context);
            }
            catch (StateMachineException)
            {
                throw;
            }
            catch (Exception ex)
            {
                context.EnqueueExecutionError(ex);
            }
            finally
            {
                await context.OnAction(ObservableAction.AfterAction, _metadata);

                await context.LogInformationAsync($"End: {this.GetType().Name}.Execute");
            }
        }
    }
}
