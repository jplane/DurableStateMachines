namespace DSM.Common.Model.Execution
{
    public interface ILogicMetadata : IExecutableContentMetadata
    {
        void Execute(dynamic data);
    }
}
