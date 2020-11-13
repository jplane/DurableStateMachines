using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;

namespace StateChartsDotNet.CoreEngine.Model.DataManipulation
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
