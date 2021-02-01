namespace DSM.Common.Model.States
{
    public interface IHistoryStateMetadata : IStateMetadata
    {
        bool IsDeep { get; }
    }
}
