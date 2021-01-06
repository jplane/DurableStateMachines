using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Diagnostics;

namespace StateChartsDotNet.Durable
{
    internal static class RegistrationExtensions
    {
        public static void RegisterStateChartInvokes(this IStateMetadata metadata,
                                                     Action<string, IInvokeStateChartMetadata> register,
                                                     string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            foreach (var invoke in metadata.GetStateChartInvokes())
            {
                var root = invoke.GetRoot();

                Debug.Assert(root != null);

                var metadataId = $"{parentId}.{root.MetadataId}";

                register(metadataId, invoke);

                root.RegisterStateChartInvokes(register, metadataId);
            }

            foreach (var state in metadata.GetStates())
            {
                state.RegisterStateChartInvokes(register, parentId);
            }
        }
    }
}
