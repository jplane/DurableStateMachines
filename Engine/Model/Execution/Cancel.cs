using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.Execution
{
    internal class Cancel : ExecutableContent
    {
        public Cancel(ICancelMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _ExecuteAsync(ExecutionContextBase context)
        {
            throw new NotImplementedException();
        }
    }
}
