using System.Collections.Generic;

namespace StateChartsDotNet.Common.Model.Data
{
    public interface IDatamodelMetadata : IModelMetadata
    {
        IEnumerable<IDataInitMetadata> GetData();
    }

    public static class DatamodelMetadataExtensions
    {
        public static void Validate(this IDatamodelMetadata metadata, Dictionary<IModelMetadata, List<string>> errors)
        {
            ((IModelMetadata) metadata).Validate(errors);

            foreach (var datainit in metadata.GetData())
            {
                datainit.Validate(errors);
            }
        }
    }
}
