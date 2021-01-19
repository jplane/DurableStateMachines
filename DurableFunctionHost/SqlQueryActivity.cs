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

namespace StateChartsDotNet.DurableFunctionHost
{
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

            var jsonConfig = context.GetInput<JObject>();

            Debug.Assert(jsonConfig != null);

            var connectionString = jsonConfig["connectionstring"].Value<string>();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("SQL query requires configured connection string.");
            }

            var query = jsonConfig["query"].Value<string>();

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new InvalidOperationException("SQL query requires configured query.");
            }

            using (var conn = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandType = CommandType.Text,
                    CommandText = query
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
