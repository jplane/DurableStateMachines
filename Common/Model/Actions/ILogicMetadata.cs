namespace DSM.Common.Model.Actions
{
    public interface ILogicMetadata : IActionMetadata
    {
        void Execute(dynamic data);
    }
}
