using System;
using System.Collections.Generic;
using DSM.Engine.Model.Actions;
using DSM.Common;
using DSM.Common.Model.States;
using DSM.Common.Model;
using System.Threading.Tasks;
using DSM.Engine;

namespace DSM.Engine.Model.States
{
    internal class StateMachine : State
    {
        private readonly Lazy<Logic> _script;

        public StateMachine(IStateMachineMetadata metadata)
            : base(metadata, null)
        {
            metadata.CheckArgNull(nameof(metadata));

            _script = new Lazy<Logic>(() =>
            {
                var meta = metadata.GetScript();

                if (meta != null)
                    return new Logic(meta);
                else
                    return null;
            });
        }

        public async Task ExecuteScript(ExecutionContextBase context)
        {
            if (_script.Value != null)
            {
                await _script.Value.ExecuteAsync(context);
            }
        }

        public bool FailFast => ((IStateMachineMetadata) _metadata).FailFast;

        public string Name => ((IStateMachineMetadata) _metadata).Id;

        public override string Id => "[ROOT]";

        public override Task InvokeAsync(ExecutionContextBase context)
        {
            throw new NotImplementedException();
        }

        public override void RecordHistory(ExecutionContextBase context)
        {
            throw new NotImplementedException();
        }
    }
}
