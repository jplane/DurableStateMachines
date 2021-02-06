namespace DSM.Common.Model.Execution
{
    public interface ILogicMetadata : IActionMetadata
    {
        void Execute(dynamic data);
    }
}
