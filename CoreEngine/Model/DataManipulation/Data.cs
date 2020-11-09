using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using CoreEngine.Model.States;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoreEngine.Model.DataManipulation
{
    internal class Data
    {
        private readonly IDataMetadata _metadata;

        public Data(IDataMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
        }

        public async Task Init(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.LogInformation("Start: Data.Init");

            try
            {
                if (!string.IsNullOrWhiteSpace(_metadata.Expression))
                {
                    var value = await context.Eval<object>(_metadata.Expression);

                    context.SetDataValue(_metadata.Id, value);
                    
                    context.LogDebug($"Set {_metadata.Id} = {value}");
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                context.EnqueueExecutionError(ex);
            }
            finally
            {
                context.LogInformation("End: Data.Init");
            }
        }
    }
}
