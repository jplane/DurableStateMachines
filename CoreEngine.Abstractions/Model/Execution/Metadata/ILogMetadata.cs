namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface ILogMetadata : IExecutableContentMetadata
    {
        string Message { get; }
    }
}
