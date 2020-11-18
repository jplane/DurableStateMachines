using DurableTask.Core;
using Microsoft.Extensions.Logging;
using StateChartsDotNet.CoreEngine;
using StateChartsDotNet.CoreEngine.Abstractions;
using StateChartsDotNet.CoreEngine.DurableTask;
using System;
using System.Threading.Tasks;

namespace CoreEngine.DurableTask
{
    public class InterpreterOrchestration : TaskOrchestration<bool, string, Message, string>
    {
        private readonly DurableExecutionContext _executionContext;

        public InterpreterOrchestration(DurableExecutionContext executionContext)
        {
            executionContext.CheckArgNull(nameof(executionContext));

            _executionContext = executionContext;
        }

        public override async Task<bool> RunTask(OrchestrationContext context, string input)
        {
            _executionContext.OrchestrationContext = context;

            _executionContext.LogInformation("Start: durable orchestration.");

            try
            {
                var interpreter = new Interpreter();

                await interpreter.Run(_executionContext);

                return true;
            }
            catch(Exception ex)
            {
                _executionContext.Logger.LogError("Error during orchestration: " + ex);

                return false;
            }
            finally
            {
                _executionContext.LogInformation("End: durable orchestration.");
            }
        }

        public override void OnEvent(OrchestrationContext context, string name, Message input)
        {
            _executionContext.EnqueueExternalMessage(input);
        }
    }
}
