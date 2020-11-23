using DurableTask.Core;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine;
using StateChartsDotNet.CoreEngine.Abstractions;
using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using StateChartsDotNet.CoreEngine.DurableTask;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CoreEngine.DurableTask
{
    public class InterpreterOrchestration : TaskOrchestration<IDictionary<string, object>,
                                                              IDictionary<string, object>,
                                                              Message,
                                                              string>
    {
        private readonly IRootStateMetadata _metadata;
        private readonly Action<string, ExecutionContext, Func<ExecutionContext, Task>> _ensureActivityRegistration;
        private readonly ILogger _logger;

        private DurableExecutionContext _executionContext;

        public InterpreterOrchestration(IRootStateMetadata metadata,
                                        Action<string, ExecutionContext, Func<ExecutionContext, Task>> ensureActivityRegistration,
                                        ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));
            ensureActivityRegistration.CheckArgNull(nameof(ensureActivityRegistration));

            _metadata = metadata;
            _ensureActivityRegistration = ensureActivityRegistration;
            _logger = logger;
        }

        public override async Task<IDictionary<string, object>> RunTask(OrchestrationContext context, IDictionary<string, object> input)
        {
            Debug.Assert(context != null);

            _executionContext = new DurableExecutionContext(_metadata,
                                                            context,
                                                            _ensureActivityRegistration,
                                                            _logger);

            if (input != null)
            {
                foreach (var pair in input)
                {
                    _executionContext[pair.Key] = pair.Value;
                }
            }

            await _executionContext.LogInformation("Start: durable orchestration.");

            try
            {
                var interpreter = new Interpreter();

                await interpreter.Run(_executionContext);
            }
            catch(Exception ex)
            {
                _logger?.LogError("Error during orchestration: " + ex);
            }
            finally
            {
                await _executionContext.LogInformation("End: durable orchestration.");
            }

            return _executionContext.GetData();
        }

        public override void OnEvent(OrchestrationContext context, string name, Message input)
        {
            Debug.Assert(_executionContext != null);

            _executionContext.EnqueueExternalMessage(input);
        }
    }
}
