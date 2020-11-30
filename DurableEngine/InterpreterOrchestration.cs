using DurableTask.Core;
using Microsoft.Extensions.Logging;
using StateChartsDotNet;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal class InterpreterOrchestration : TaskOrchestration<IDictionary<string, object>,
                                                              IDictionary<string, object>,
                                                              ExternalMessage,
                                                              string>
    {
        private readonly IRootStateMetadata _metadata;
        private readonly Action<string, Func<TaskActivity>> _ensureActivityRegistration;
        private readonly Action<string, Func<InterpreterOrchestration>> _ensureOrchestrationRegistration;
        private readonly IReadOnlyDictionary<string, IRootStateMetadata> _childMetadata;
        private readonly IReadOnlyDictionary<string, ExternalServiceDelegate> _externalServices;
        private readonly IReadOnlyDictionary<string, ExternalQueryDelegate> _externalQueries;
        private readonly ILogger _logger;

        private DurableExecutionContext _executionContext;

        public InterpreterOrchestration(IRootStateMetadata metadata,
                                        Action<string, Func<TaskActivity>> ensureActivityRegistration,
                                        Action<string, Func<InterpreterOrchestration>> ensureOrchestrationRegistration,
                                        IReadOnlyDictionary<string, IRootStateMetadata> childMetadata,
                                        IReadOnlyDictionary<string, ExternalServiceDelegate> externalServices,
                                        IReadOnlyDictionary<string, ExternalQueryDelegate> externalQueries,
                                        ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));
            ensureActivityRegistration.CheckArgNull(nameof(ensureActivityRegistration));
            ensureOrchestrationRegistration.CheckArgNull(nameof(ensureOrchestrationRegistration));
            childMetadata.CheckArgNull(nameof(childMetadata));
            externalServices.CheckArgNull(nameof(externalServices));
            externalQueries.CheckArgNull(nameof(externalQueries));

            _metadata = metadata;
            _ensureActivityRegistration = ensureActivityRegistration;
            _ensureOrchestrationRegistration = ensureOrchestrationRegistration;
            _childMetadata = childMetadata;
            _externalServices = externalServices;
            _externalQueries = externalQueries;
            _logger = logger;
        }

        public override async Task<IDictionary<string, object>> RunTask(OrchestrationContext context, IDictionary<string, object> input)
        {
            Debug.Assert(context != null);

            _executionContext = new DurableExecutionContext(_metadata,
                                                            context,
                                                            _ensureActivityRegistration,
                                                            _ensureOrchestrationRegistration,
                                                            _childMetadata,
                                                            _externalServices,
                                                            _externalQueries,
                                                            _logger);

            if (input != null)
            {
                foreach (var pair in input)
                {
                    _executionContext[pair.Key] = pair.Value;
                }
            }

            await _executionContext.LogInformationAsync("Start: durable orchestration.");

            try
            {
                var interpreter = new StateChartsDotNet.Interpreter();

                await interpreter.RunAsync(_executionContext);
            }
            catch(Exception ex)
            {
                _logger?.LogError("Error during orchestration: " + ex);
            }
            finally
            {
                await _executionContext.LogInformationAsync("End: durable orchestration.");
            }

            return _executionContext.GetData();
        }

        public override void OnEvent(OrchestrationContext context, string name, ExternalMessage input)
        {
            Debug.Assert(_executionContext != null);

            _executionContext.EnqueueExternalMessage(input);
        }
    }
}
