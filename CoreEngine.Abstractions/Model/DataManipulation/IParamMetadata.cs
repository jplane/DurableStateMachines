namespace StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation
{
    public interface IParamMetadata
    {
        string Name { get; }
        string Location { get; }
        string Expression { get; }
    }
}
