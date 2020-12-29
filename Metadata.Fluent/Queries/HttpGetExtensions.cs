using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Metadata.Fluent.Execution;
using StateChartsDotNet.Metadata.Fluent.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StateChartsDotNet.Metadata.Fluent.Queries.HttpGet
{
    public static class HttpGetExtensions
    {
        public static QueryMetadata<OnEntryExitMetadata<TParent>> HttpGet<TParent>(this OnEntryExitMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var query = metadata.Query();

            query.Type("http-get");

            return query;
        }

        public static QueryMetadata<IfMetadata<TParent>> HttpGet<TParent>(this IfMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var query = metadata.Query();

            query.Type("http-get");

            return query;
        }

        public static QueryMetadata<ElseIfMetadata<TParent>> HttpGet<TParent>(this ElseIfMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var query = metadata.Query();

            query.Type("http-get");

            return query;
        }

        public static QueryMetadata<ElseMetadata<TParent>> HttpGet<TParent>(this ElseMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var query = metadata.Query();

            query.Type("http-get");

            return query;
        }

        public static QueryMetadata<ForeachMetadata<TParent>> HttpGet<TParent>(this ForeachMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var query = metadata.Query();

            query.Type("http-get");

            return query;
        }

        public static QueryMetadata<InvokeStateChartMetadata<TParent>> HttpGet<TParent>(this InvokeStateChartMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var query = metadata.Query();

            query.Type("http-get");

            return query;
        }

        public static QueryMetadata<TransitionMetadata<TParent>> HttpGet<TParent>(this TransitionMetadata<TParent> metadata)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var query = metadata.Query();

            query.Type("http-get");

            return query;
        }

        public static QueryMetadata<TParent> Url<TParent>(this QueryMetadata<TParent> metadata, string url)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Target(url);

            return metadata;
        }

        public static QueryMetadata<TParent> Url<TParent>(this QueryMetadata<TParent> metadata, Func<dynamic, string> getter)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            metadata.Target(getter);

            return metadata;
        }

        public static QueryMetadata<TParent> QueryStringNameValue<TParent>(this QueryMetadata<TParent> metadata,
                                                                           string name,
                                                                           string value)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var parm = metadata.Param($"?{name}");

            parm.Value(value);

            return metadata;
        }

        public static QueryMetadata<TParent> QueryStringNameValue<TParent>(this QueryMetadata<TParent> metadata,
                                                                           string name,
                                                                           Func<dynamic, string> getter)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var parm = metadata.Param($"?{name}");

            parm.Value(getter);

            return metadata;
        }

        public static QueryMetadata<TParent> HeaderNameValue<TParent>(this QueryMetadata<TParent> metadata,
                                                                            string name,
                                                                            string value)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var parm = metadata.Param(name);

            parm.Value(value);

            return metadata;
        }

        public static QueryMetadata<TParent> HeaderNameValue<TParent>(this QueryMetadata<TParent> metadata,
                                                                      string name,
                                                                      Func<dynamic, string> getter)
            where TParent : IModelMetadata
        {
            metadata.CheckArgNull(nameof(metadata));

            var parm = metadata.Param(name);

            parm.Value(getter);

            return metadata;
        }
    }
}
