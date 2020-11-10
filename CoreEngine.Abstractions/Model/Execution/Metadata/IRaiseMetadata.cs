namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface IRaiseMetadata : IExecutableContentMetadata
    {
        string Event { get; }
    }
}
