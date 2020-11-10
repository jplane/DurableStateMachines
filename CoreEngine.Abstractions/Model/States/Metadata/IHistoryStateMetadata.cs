namespace CoreEngine.Abstractions.Model.States.Metadata
{
    public interface IHistoryStateMetadata : IStateMetadata
    {
        HistoryType Type { get; }
    }
}
