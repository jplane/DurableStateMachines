namespace CoreEngine.Abstractions.Model.DataManipulation.Metadata
{
    public interface IDataMetadata
    {
        string Id { get; }
        string Source { get; }
        string Expression { get; }
        string Body { get; }
    }
}
