namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IScriptMetadata : IExecutableContentMetadata
    {
        void Execute(dynamic data);
    }
}
