namespace StateChartsDotNet.Common.Model.Execution
{
    public interface ICancelMetadata : IExecutableContentMetadata
    {
        string SendId { get; }
        string SendIdExpr { get; }
    }
}
