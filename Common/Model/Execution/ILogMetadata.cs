namespace StateChartsDotNet.Common.Model.Execution
{
    public interface ILogMetadata : IExecutableContentMetadata
    {
        string GetMessage(dynamic data);
    }
}
