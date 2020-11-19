using DurableTask.Core;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine;
using StateChartsDotNet.CoreEngine.Abstractions;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.DurableTask;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CoreEngine.DurableTask
{
    public class InterpreterOrchestration : TaskOrchestration<bool, string, Message, string>
    {
        private readonly IModelMetadata _metadata;
        private readonly Action<string, ExecutionContext, Func<ExecutionContext, Task>> _ensureActivityRegistration;
        private readonly ILogger _logger;

        private DurableExecutionContext _executionContext;

        public InterpreterOrchestration(IModelMetadata metadata,
                                        Action<string, ExecutionContext, Func<ExecutionContext, Task>> ensureActivityRegistration,
                                        ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));
            ensureActivityRegistration.CheckArgNull(nameof(ensureActivityRegistration));

            _metadata = metadata;
            _ensureActivityRegistration = ensureActivityRegistration;
            _logger = logger;
        }

        public override async Task<bool> RunTask(OrchestrationContext context, string input)
        {
            _executionContext = new DurableExecutionContext(_metadata,
                                                            context,
                                                            _ensureActivityRegistration);

            _executionContext.Logger = _logger;

            await _executionContext.LogInformation("Start: durable orchestration.");

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
                await _executionContext.LogInformation("End: durable orchestration.");
            }
        }

        public override void OnEvent(OrchestrationContext context, string name, Message input)
        {
            Debug.Assert(_executionContext != null);

            _executionContext.EnqueueExternalMessage(input);
        }
    }
}
