using DSM.Common.Model.Execution;

namespace DSM.Common.Model.States
{
    public interface IStateChartMetadata : IStateMetadata
    {
        bool FailFast { get; }

        IScriptMetadata GetScript();
        void Validate();
    }
}
