namespace DSM.Common.Model.Actions
{
    public interface ILogMetadata : IActionMetadata
    {
        string GetMessage(dynamic data);
    }
}
