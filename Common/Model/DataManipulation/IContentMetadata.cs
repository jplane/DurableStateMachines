namespace StateChartsDotNet.Common.Model.DataManipulation
{
    public interface IContentMetadata : IModelMetadata
    {
        string Expression { get; }
        string Body { get; }
    }
}
