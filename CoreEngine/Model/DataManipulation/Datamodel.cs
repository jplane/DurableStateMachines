using System;
using System.Linq;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;

namespace StateChartsDotNet.CoreEngine.Model.DataManipulation
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

        public void Init(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.LogInformation("Start: Datamodel.Init");

            try
            {
                foreach (var data in _data.Value)
                {
                    data.Init(context);
                }
            }
            finally
            {
                context.LogInformation("End: Datamodel.Init");
            }
        }
    }
}
