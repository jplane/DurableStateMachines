using DSM.Common.Model.Actions;

namespace DSM.Common.Model.States
{
    public interface IStateMachineMetadata : IStateMetadata
    {
        bool FailFast { get; }

        ILogicMetadata GetScript();
        void Validate();
    }
}
