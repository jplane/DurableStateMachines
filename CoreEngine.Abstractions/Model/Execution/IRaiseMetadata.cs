namespace StateChartsDotNet.CoreEngine.Abstractions.Model.Execution
{
    public interface IRaiseMetadata : IExecutableContentMetadata
    {
        string Event { get; }
    }
}
