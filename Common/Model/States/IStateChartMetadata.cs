using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.Execution;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IStateChartMetadata : IStateMetadata
    {
        bool FailFast { get; }

        IScriptMetadata GetScript();
        IDataModelMetadata GetDataModel();
        void Validate();
    }
}
