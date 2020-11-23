namespace StateChartsDotNet.Common.Model.DataManipulation
{
    public interface IDataInitMetadata : IModelMetadata
    {
        string Id { get; }
        object GetValue(dynamic data);
    }
}
