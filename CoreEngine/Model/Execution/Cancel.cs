using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using System;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine.Model.Execution
{
    internal class Cancel : ExecutableContent
    {
        public Cancel(ICancelMetadata metadata)
            : base(metadata)
        {
        }

        protected override Task _Execute(ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
