using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Exceptions;
using StateChartsDotNet.Common.Model.Execution;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Common.Model.States
{
    public interface IStateChartMetadata : IStateMetadata
    {
        bool FailFast { get; }
        Databinding Databinding { get; }

        IScriptMetadata GetScript();

        Task<string> SerializeAsync(Stream stream, CancellationToken cancelToken = default);
        Task<(string, string)> ToStringAsync(CancellationToken cancelToken = default);
    }

    public static class StateChartMetadataExtensions
    {
        public static void Validate(this IStateChartMetadata metadata)
        {
            var errors = new Dictionary<IModelMetadata, List<string>>();

            ((IStateMetadata) metadata).Validate(errors);

            metadata.GetScript()?.Validate(errors);

            if (errors.Any())
            {
                throw new MetadataValidationException(errors.ToDictionary(p => p.Key.MetadataId, p => p.Value.ToArray()));
            }
        }
    }
}
