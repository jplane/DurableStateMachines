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
        private readonly IStateChartMetadata _metadata;
        private readonly CancellationToken _cancelToken;
        private readonly ILogger _logger;
        private readonly bool _executeInline;
        
        private DurableExecutionContext _executionContext;

        public InterpreterOrchestration(IStateChartMetadata metadata,
                                        CancellationToken cancelToken,
                                        bool executeInline,
                                        ILogger logger = null)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
            _cancelToken = cancelToken;
            _executeInline = executeInline;
            _logger = logger;

            this.DataConverter = new JsonDataConverter(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All                
            });
        }

        private void InitExecutionContext(OrchestrationContext context, Dictionary<string, object> data)
        {
            Debug.Assert(context != null);
            Debug.Assert(data != null);

            if (_executeInline)
            {
                _executionContext = new InlineDurableExecutionContext(_metadata, context, data, _cancelToken, _logger);
            }
            else
            {
                _executionContext = new DurableExecutionContext(_metadata, context, data, _cancelToken, _logger);
            }
        }

        public override async Task<(Dictionary<string, object>, Exception)> RunTask(OrchestrationContext context, Dictionary<string, object> data)
        {
            data.CheckArgNull(nameof(data));

            Debug.Assert(context != null);

            InitExecutionContext(context, data);

            Debug.Assert(_executionContext != null);

            await _executionContext.LogInformationAsync("Start: durable orchestration.");

            try
            {
                var interpreter = new Interpreter();

                await interpreter.RunAsync(_executionContext);

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
