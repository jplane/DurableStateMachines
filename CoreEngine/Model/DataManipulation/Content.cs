using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using System;
using System.Xml.Linq;

namespace CoreEngine.Model.DataManipulation
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
