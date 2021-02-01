﻿using DSM.Common.Model.Execution;

namespace DSM.Common.Model.States
{
    public interface IStateMachineMetadata : IStateMetadata
    {
        bool FailFast { get; }

        IScriptMetadata GetScript();
        void Validate();
    }
}
