namespace StateChartsDotNet.CoreEngine.Abstractions.Model.Execution
{
    public interface ICancelMetadata : IExecutableContentMetadata
    {
        string SendId { get; }
        string SendIdExpr { get; }
    }
}
