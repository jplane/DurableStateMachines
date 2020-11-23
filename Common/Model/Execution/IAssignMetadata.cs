namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IAssignMetadata : IExecutableContentMetadata
    {
        string Location { get; }
        object GetValue(dynamic data);
    }
}
