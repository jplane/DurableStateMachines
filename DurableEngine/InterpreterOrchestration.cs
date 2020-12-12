using DurableTask.Core;
using DurableTask.Core.Serializing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StateChartsDotNet;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Durable
{
    internal class InterpreterOrchestration : TaskOrchestration<(Dictionary<string, object>, Exception),
                                                                Dictionary<string, object>,
                                                                ExternalMessage,
                                                                string>
    {
        private readonly IRootStateMetadata _metadata;
        private readonly CancellationToken _cancelToken;
        private readonly ILogger _logger;
        
        private DurableExecutionContext _executionContext;

        public InterpreterOrchestration(IRootStateMetadata metadata,
                                        CancellationToken cancelToken,
                                        ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
            _cancelToken = cancelToken;
            _logger = logger;

            this.DataConverter = new JsonDataConverter(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All                
            });
        }

        public override async Task<(Dictionary<string, object>, Exception)> RunTask(OrchestrationContext context, Dictionary<string, object> data)
        {
            data.CheckArgNull(nameof(data));

            Debug.Assert(context != null);

            _executionContext = new DurableExecutionContext(_metadata, context, data, _logger);

            await _executionContext.LogInformationAsync("Start: durable orchestration.");

            try
            {
                var interpreter = new StateChartsDotNet.Interpreter();

                await interpreter.RunAsync(_executionContext, _cancelToken);

                return (_executionContext.ResultData, null);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
            finally
            {
                await _executionContext.LogInformationAsync("End: durable orchestration.");
            }
        }

        public override void OnEvent(OrchestrationContext context, string name, ExternalMessage input)
        {
            Debug.Assert(_executionContext != null);

            _executionContext.EnqueueExternalMessage(input);
        }
    }
}
