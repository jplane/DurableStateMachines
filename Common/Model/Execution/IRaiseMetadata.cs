namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IRaiseMetadata : IExecutableContentMetadata
    {
        string MessageName { get; }
    }
}
