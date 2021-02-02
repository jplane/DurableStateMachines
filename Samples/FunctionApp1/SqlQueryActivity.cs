using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DSM.Common;
using DSM.Common.Model.Execution;

namespace DSM.DurableFunction.Host
{
    public class SqlQueryConfiguration : IQueryConfiguration
    {
        [JsonProperty("connectionstring")]
        public string ConnectionString { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }
    }

    public class SqlQueryActivity
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SqlQueryActivity> _logger;

        public SqlQueryActivity(IConfiguration config, ILogger<SqlQueryActivity> logger)
        {
            _config = config;
            _logger = logger;
        }

        [FunctionName("sql-query")]
        public async Task<string> SqlQuery([ActivityTrigger] IDurableActivityContext context)
        {
            Debug.Assert(context != null);

            var config = context.GetInput<SqlQueryConfiguration>();

            if (config == null)
            {
                throw new InvalidOperationException("Missing configuration for sql query activity.");
            }

            if (string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                throw new InvalidOperationException("Missing connection string for sql query activity.");
            }

            if (string.IsNullOrWhiteSpace(config.Query))
            {
                throw new InvalidOperationException("Missing query for sql query activity.");
            }

            using (var conn = new SqlConnection(config.ConnectionString))
            {
                var cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.Text,
                    CommandText = config.Query
                };

                await conn.OpenAsync().ConfigureAwait(false);

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var schema = reader.GetColumnSchema();

                    var rows = new List<JObject>();

                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var row = new JObject();

                        foreach (var col in schema)
                        {
                            row[col.ColumnName] = JToken.FromObject(reader[col.ColumnName]);
                        }

                        rows.Add(row);
                    }

                    return JsonConvert.SerializeObject(rows);
                }
            }
        }
    }
}
