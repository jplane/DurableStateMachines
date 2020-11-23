namespace StateChartsDotNet.Common.Model.States
{
    public interface IHistoryStateMetadata : IStateMetadata
    {
        HistoryType Type { get; }
    }
}
