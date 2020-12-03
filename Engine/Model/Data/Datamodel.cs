using System;
using System.Linq;
using System.Threading.Tasks;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Data;

namespace StateChartsDotNet.Model.Data
{
    internal class Datamodel
    {
        protected readonly Lazy<DataInit[]> _data;

        public Datamodel(IDatamodelMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _data = new Lazy<DataInit[]>(() =>
            {
                return metadata.GetData().Select(d => new DataInit(d)).ToArray();
            });
        }

        public async Task Init(ExecutionContextBase context)
        {
            context.CheckArgNull(nameof(context));

            await context.LogInformationAsync("Start: Datamodel.Init");

            try
            {
                foreach (var data in _data.Value)
                {
                    await data.Init(context);
                }
            }
            catch (Exception ex)
            {
                context.EnqueueExecutionError(ex);
            }
            finally
            {
                await context.LogInformationAsync("End: Datamodel.Init");
            }
        }
    }
}
