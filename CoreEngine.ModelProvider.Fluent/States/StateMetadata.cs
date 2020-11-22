﻿using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.States
{
    public abstract class StateMetadata : IStateMetadata
    {
        private readonly string _id;

        internal StateMetadata(string id)
        {
            _id = id;
        }

        protected abstract IStateMetadata _Parent { get; }

        internal virtual IEnumerable<string> StateNames
        {
            get => new[] { _id };
        }

        string IStateMetadata.Id => _id;

        string IModelMetadata.UniqueId => _id;

        int IStateMetadata.DepthFirstCompare(IStateMetadata metadata)
        {
            int Compare(string x, string y)
            {
                return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
            }

            if (Compare(((IStateMetadata) this).Id, metadata.Id) == 0)
            {
                return 0;
            }

            var current = this;
            var parent = this._Parent;

            while (parent != null)
            {
                Debug.Assert(parent is StateMetadata);

                current = (StateMetadata) parent;

                parent = current._Parent;
            }

            Debug.Assert(current is RootStateMetadata);

            var stateNames = ((RootStateMetadata)current).StateNames.ToArray();

            foreach (var name in stateNames)
            {
                if (Compare(((IStateMetadata)this).Id, name) == 0)
                {
                    return 1;
                }
                else if (Compare(metadata.Id, name) == 0)
                {
                    return -1;
                }
            }

            Debug.Fail("Unexpected result.");

            return 0;
        }

        protected virtual IDatamodelMetadata GetDatamodel() => null;

        IDatamodelMetadata IStateMetadata.GetDatamodel()
        {
            return GetDatamodel();
        }

        protected virtual IOnEntryExitMetadata GetOnEntry() => null;

        IOnEntryExitMetadata IStateMetadata.GetOnEntry()
        {
            return GetOnEntry();
        }

        protected virtual IOnEntryExitMetadata GetOnExit() => null;

        IOnEntryExitMetadata IStateMetadata.GetOnExit()
        {
            return GetOnExit();
        }

        protected virtual IEnumerable<IInvokeStateChartMetadata> GetStateChartInvokes() => Enumerable.Empty<IInvokeStateChartMetadata>();

        IEnumerable<IInvokeStateChartMetadata> IStateMetadata.GetStateChartInvokes()
        {
            return GetStateChartInvokes();
        }

        protected virtual IEnumerable<ITransitionMetadata> GetTransitions() => Enumerable.Empty<ITransitionMetadata>();

        IEnumerable<ITransitionMetadata> IStateMetadata.GetTransitions()
        {
            return GetTransitions();
        }

        bool IStateMetadata.IsDescendentOf(IStateMetadata state)
        {
            var parent = this._Parent;

            while (parent != null)
            {
                Debug.Assert(parent is StateMetadata);

                if (parent == state)
                {
                    return true;
                }

                parent = ((StateMetadata) parent)._Parent;
            }

            return false;
        }

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new NotImplementedException();
        }
    }
}
