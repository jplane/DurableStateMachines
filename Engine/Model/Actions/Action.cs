using DSM.Common;
using DSM.Common.Exceptions;
using DSM.Common.Model.Actions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DSM.Engine;
using DSM.Common.Observability;

namespace DSM.Engine.Model.Actions
{
    internal abstract class Action
    {
        protected readonly IActionMetadata _metadata;

        protected Action(IActionMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
        }

        public static Action Create(IActionMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            Action action = null;

            switch (metadata)
            {
                case IIfMetadata @if:
                    action = new If(@if);
                    break;
                case IRaiseMetadata raise:
                    action = new Raise(raise);
                    break;
                case ILogicMetadata logic:
                    action = new Logic(logic);
                    break;
                case IForeachMetadata @foreach:
                    action = new Foreach(@foreach);
                    break;
                case ILogMetadata log:
                    action = new Log(log);
                    break;
                case ISendMessageMetadata send:
                    action = new SendMessage(send);
                    break;
                case IAssignMetadata assign:
                    action = new Assign(assign);
                    break;
                case IQueryMetadata query:
                    action = new Query(query);
                    break;
                case IInvokeStateMachineMetadata invoke:
                    action = new InvokeStateMachine(invoke);
                    break;
            }

            Debug.Assert(action != null, $"Action is not a recognized type: {metadata.GetType().FullName}");

            return action;
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
