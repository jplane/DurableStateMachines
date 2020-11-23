using StateChartsDotNet.Common.Model.DataManipulation;
using System;

namespace StateChartsDotNet.Model.DataManipulation
{
    internal class DataInit
    {
        private readonly IDataInitMetadata _metadata;

        public DataInit(IDataInitMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
        }

        public void Init(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.LogInformation("Start: DataInit");

            try
            {
                var value = _metadata.GetValue(context.ScriptData);

                context.SetDataValue(_metadata.Id, value);

                context.LogDebug($"Set {_metadata.Id} = {value}");
            }
            catch (Exception ex)
            {
                context.EnqueueExecutionError(ex);
            }
            finally
            {
                context.LogInformation("End: DataInit");
            }
        }
    }
}
