namespace StateChartsDotNet.Common.Model.States
{
    public interface IHistoryStateMetadata : IStateMetadata
    {
        bool IsDeep { get; }
    }
}
