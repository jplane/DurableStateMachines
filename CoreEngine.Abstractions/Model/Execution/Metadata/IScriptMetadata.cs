namespace CoreEngine.Abstractions.Model.Execution.Metadata
{
    public interface IScriptMetadata : IExecutableContentMetadata
    {
        string Source { get; }
        string BodyExpression { get; }
    }
}
