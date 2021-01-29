using System;
using System.Collections.Generic;
using StateChartsDotNet.Model.Execution;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Common.Model;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.States
{
    internal class StateChart<TData> : State<TData>
    {
        private readonly Lazy<Script<TData>> _script;

        public StateChart(IStateChartMetadata metadata)
            : base(metadata, null)
        {
            metadata.CheckArgNull(nameof(metadata));

            _script = new Lazy<Script<TData>>(() =>
            {
                var meta = metadata.GetScript();

                if (meta != null)
                    return new Script<TData>(meta);
                else
                    return null;
            });
        }

        public async Task ExecuteScript(ExecutionContextBase<TData> context)
        {
            if (_script.Value != null)
            {
                await _script.Value.ExecuteAsync(context);
            }
        }

        public bool FailFast => ((IStateChartMetadata) _metadata).FailFast;

        public string Name => ((IStateChartMetadata) _metadata).Id;

        public override string Id => "[ROOT]";

        public override Task InvokeAsync(ExecutionContextBase<TData> context)
        {
            throw new NotImplementedException();
        }

        public override void RecordHistory(ExecutionContextBase<TData> context)
        {
            throw new NotImplementedException();
        }
    }
}
