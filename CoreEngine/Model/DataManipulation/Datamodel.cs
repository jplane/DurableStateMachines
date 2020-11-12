using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using CoreEngine.Abstractions.Model.DataManipulation.Metadata;

namespace CoreEngine.Model.DataManipulation
{
    internal class Datamodel
    {
        protected readonly AsyncLazy<DataInit[]> _data;

        public Datamodel(IDatamodelMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _data = new AsyncLazy<DataInit[]>(async () =>
            {
                return (await metadata.GetData()).Select(d => new DataInit(d)).ToArray();
            });
        }

        public async Task Init(ExecutionContext context)
        {
            context.CheckArgNull(nameof(context));

            context.LogInformation("Start: Datamodel.Init");

            try
            {
                foreach (var data in await _data)
                {
                    await data.Init(context);
                }
            }
            finally
            {
                context.LogInformation("End: Datamodel.Init");
            }
        }
    }
}
