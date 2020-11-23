using StateChartsDotNet.Common.Model.DataManipulation;

namespace StateChartsDotNet.Model.DataManipulation
{
    internal class Param
    {
        private readonly IParamMetadata _metadata;

        public Param(IParamMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
        }
    }
}
