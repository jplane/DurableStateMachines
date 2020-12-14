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
        public static void RegisterStateChartInvokes(this IRootStateMetadata metadata, Action<string, IRootStateMetadata, bool> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            foreach (var state in metadata.GetStates())
            {
                state.RegisterStateChartInvokes(register);
            }
        }

        public static void RegisterScripts(this IRootStateMetadata metadata, Action<IScriptMetadata> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            var script = metadata.GetScript();

            if (script != null)
            {
                register(metadata.GetScript());
            }

            foreach (var state in metadata.GetStates())
            {
                state.RegisterScripts(register);
            }
        }

        public static void RegisterStateChartInvokes(this IStateMetadata metadata, Action<string, IRootStateMetadata, bool> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            foreach (var invoke in metadata.GetStateChartInvokes())
            {
                var root = invoke.GetRoot();

                register(invoke.UniqueId, root, invoke.Autoforward);

                root.RegisterStateChartInvokes(register);
            }
        }

        public static void RegisterScripts(this IStateMetadata metadata, Action<IScriptMetadata> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            foreach (var invoke in metadata.GetStateChartInvokes())
            {
                invoke.GetRoot().RegisterScripts(register);

                foreach (var content in invoke.GetFinalizeExecutableContent())
                {
                    content.RegisterScripts(register);
                }
            }

            foreach (var transition in metadata.GetTransitions())
            {
                foreach (var content in transition.GetExecutableContent())
                {
                    content.RegisterScripts(register);
                }
            }

            var onEntry = metadata.GetOnEntry();

            if (onEntry != null)
            {
                foreach (var content in onEntry.GetExecutableContent())
                {
                    content.RegisterScripts(register);
                }
            }

            var onExit = metadata.GetOnExit();

            if (onExit != null)
            {
                foreach (var content in onExit.GetExecutableContent())
                {
                    content.RegisterScripts(register);
                }
            }
        }

        public static void RegisterStateChartInvokes(this ISequentialStateMetadata metadata, Action<string, IRootStateMetadata, bool> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            RegisterStateChartInvokes((IStateMetadata) metadata, register);

            foreach (var state in metadata.GetStates())
            {
                state.RegisterStateChartInvokes(register);
            }
        }

        public static void RegisterScripts(this ISequentialStateMetadata metadata, Action<IScriptMetadata> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            RegisterScripts((IStateMetadata) metadata, register);

            foreach (var state in metadata.GetStates())
            {
                state.RegisterScripts(register);
            }
        }

        public static void RegisterStateChartInvokes(this IParallelStateMetadata metadata, Action<string, IRootStateMetadata, bool> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            RegisterStateChartInvokes((IStateMetadata) metadata, register);

            foreach (var state in metadata.GetStates())
            {
                state.RegisterStateChartInvokes(register);
            }
        }

        public static void RegisterScripts(this IParallelStateMetadata metadata, Action<IScriptMetadata> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            RegisterScripts((IStateMetadata) metadata, register);

            foreach (var state in metadata.GetStates())
            {
                state.RegisterScripts(register);
            }
        }

        public static void RegisterScripts(this IQueryMetadata metadata, Action<IScriptMetadata> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            foreach (var content in metadata.GetExecutableContent())
            {
                content.RegisterScripts(register);
            }
        }

        public static void RegisterScripts(this IIfMetadata metadata, Action<IScriptMetadata> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            foreach (var content in metadata.GetExecutableContent())
            {
                content.RegisterScripts(register);
            }

            foreach (var content in metadata.GetElseIfExecutableContent().SelectMany(ienum => ienum))
            {
                content.RegisterScripts(register);
            }

            foreach (var content in metadata.GetElseExecutableContent())
            {
                content.RegisterScripts(register);
            }
        }

        public static void RegisterScripts(this IForeachMetadata metadata, Action<IScriptMetadata> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            foreach (var content in metadata.GetExecutableContent())
            {
                content.RegisterScripts(register);
            }
        }

        public static void RegisterScripts(this IExecutableContentMetadata metadata, Action<IScriptMetadata> register)
        {
            metadata.CheckArgNull(nameof(metadata));
            register.CheckArgNull(nameof(register));

            if (metadata is IScriptMetadata script)
            {
                register(script);
            }
        }
    }
}
