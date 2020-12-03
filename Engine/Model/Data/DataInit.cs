using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Data;
using System;
using System.Threading.Tasks;

namespace StateChartsDotNet.Model.Data
{
    internal class DataInit
    {
        private readonly IDataInitMetadata _metadata;

        public DataInit(IDataInitMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
        }

        public async Task Init(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            var value = _metadata.GetValue(context.ScriptData);

            context.SetDataValue(_metadata.Id, value);

            await context.LogDebugAsync($"Set {_metadata.Id} = {value}");
        }
    }
}
