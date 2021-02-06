namespace DSM.Common.Model.Execution
{
    public interface ILogMetadata : IActionMetadata
    {
        string GetMessage(dynamic data);
    }
}
