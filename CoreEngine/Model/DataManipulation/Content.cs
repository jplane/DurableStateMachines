using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;

namespace StateChartsDotNet.CoreEngine.Model.DataManipulation
{
    internal class Content
    {
        private readonly IContentMetadata _metadata;

        public Content(IContentMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _metadata = metadata;
        }
    }
}
