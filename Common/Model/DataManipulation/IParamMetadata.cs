namespace StateChartsDotNet.Common.Model.DataManipulation
{
    public interface IParamMetadata : IModelMetadata
    {
        string Name { get; }
        object GetValue(dynamic data);
    }
}
