using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateChartsDotNet.Durable
{
    internal static class RegistrationExtensions
    {
        public static void RegisterStateChartInvokes(this IStateChartMetadata metadata, Action<string, IStateChartMetadata, bool> register,
                                                     string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            foreach (var state in metadata.GetStates())
            {
                state.RegisterStateChartInvokes(register, parentId);
            }
        }

        public static void RegisterScripts(this IStateChartMetadata metadata, Action<string, IScriptMetadata> register, string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            var script = metadata.GetScript();

            if (script != null)
            {
                register($"{parentId}.{metadata.UniqueId}", metadata.GetScript());
            }

            foreach (var state in metadata.GetStates())
            {
                state.RegisterScripts(register, parentId);
            }
        }

        public static void RegisterStateChartInvokes(this IStateMetadata metadata,
                                                     Action<string, IStateChartMetadata, bool> register,
                                                     string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            foreach (var invoke in metadata.GetStateChartInvokes())
            {
                var root = invoke.GetRoot();

                var uniqueId = $"{parentId}.{root.UniqueId}";

                register(uniqueId, root, invoke.Autoforward);

                root.RegisterStateChartInvokes(register, uniqueId);
            }
        }

        public static void RegisterScripts(this IStateMetadata metadata, Action<string, IScriptMetadata> register, string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            foreach (var invoke in metadata.GetStateChartInvokes())
            {
                var root = invoke.GetRoot();
                
                root.RegisterScripts(register, $"{parentId}.{root.UniqueId}");

                foreach (var content in invoke.GetFinalizeExecutableContent())
                {
                    content.RegisterScripts(register, parentId);
                }
            }

            foreach (var transition in metadata.GetTransitions())
            {
                foreach (var content in transition.GetExecutableContent())
                {
                    content.RegisterScripts(register, parentId);
                }
            }

            var onEntry = metadata.GetOnEntry();

            if (onEntry != null)
            {
                foreach (var content in onEntry.GetExecutableContent())
                {
                    content.RegisterScripts(register, parentId);
                }
            }

            var onExit = metadata.GetOnExit();

            if (onExit != null)
            {
                foreach (var content in onExit.GetExecutableContent())
                {
                    content.RegisterScripts(register, parentId);
                }
            }
        }

        public static void RegisterStateChartInvokes(this ISequentialStateMetadata metadata,
                                                     Action<string, IStateChartMetadata, bool> register,
                                                     string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            RegisterStateChartInvokes((IStateMetadata) metadata, register, parentId);

            foreach (var state in metadata.GetStates())
            {
                state.RegisterStateChartInvokes(register, parentId);
            }
        }

        public static void RegisterScripts(this ISequentialStateMetadata metadata, Action<string, IScriptMetadata> register, string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            RegisterScripts((IStateMetadata) metadata, register, parentId);

            foreach (var state in metadata.GetStates())
            {
                state.RegisterScripts(register, parentId);
            }
        }

        public static void RegisterStateChartInvokes(this IParallelStateMetadata metadata,
                                                     Action<string, IStateChartMetadata, bool> register,
                                                     string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            RegisterStateChartInvokes((IStateMetadata) metadata, register, parentId);

            foreach (var state in metadata.GetStates())
            {
                state.RegisterStateChartInvokes(register, parentId);
            }
        }

        public static void RegisterScripts(this IParallelStateMetadata metadata, Action<string, IScriptMetadata> register, string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            RegisterScripts((IStateMetadata) metadata, register, parentId);

            foreach (var state in metadata.GetStates())
            {
                state.RegisterScripts(register, parentId);
            }
        }

        public static void RegisterScripts(this IQueryMetadata metadata, Action<string, IScriptMetadata> register, string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            foreach (var content in metadata.GetExecutableContent())
            {
                content.RegisterScripts(register, parentId);
            }
        }

        public static void RegisterScripts(this IIfMetadata metadata, Action<string, IScriptMetadata> register, string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            foreach (var content in metadata.GetExecutableContent())
            {
                content.RegisterScripts(register, parentId);
            }

            foreach (var content in metadata.GetElseIfExecutableContent().SelectMany(ienum => ienum))
            {
                content.RegisterScripts(register, parentId);
            }

            foreach (var content in metadata.GetElseExecutableContent())
            {
                content.RegisterScripts(register, parentId);
            }
        }

        public static void RegisterScripts(this IForeachMetadata metadata, Action<string, IScriptMetadata> register, string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            foreach (var content in metadata.GetExecutableContent())
            {
                content.RegisterScripts(register, parentId);
            }
        }

        public static void RegisterScripts(this IExecutableContentMetadata metadata, Action<string, IScriptMetadata> register, string parentId)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));
            parentId.CheckArgNull(nameof(parentId));

            if (metadata is IScriptMetadata script)
            {
                register($"{parentId}.{script.UniqueId}", script);
            }
        }
    }
}
