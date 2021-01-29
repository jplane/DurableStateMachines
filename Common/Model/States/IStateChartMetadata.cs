using StateChartsDotNet.Common.Model.Execution;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IStateChartMetadata : IStateMetadata
    {
        bool FailFast { get; }

        IScriptMetadata GetScript();
        void Validate();
    }
}
