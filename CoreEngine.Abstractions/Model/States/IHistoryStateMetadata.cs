namespace StateChartsDotNet.CoreEngine.Abstractions.Model.States
{
    public interface IHistoryStateMetadata : IStateMetadata
    {
        HistoryType Type { get; }
    }
}
