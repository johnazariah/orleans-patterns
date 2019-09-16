using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Patterns.Utilities
{
    public static partial class Extensions
    {
        public static string StringOrDefault(this string s, string defaultValue) =>
            string.IsNullOrWhiteSpace(s) ? defaultValue : s;

        public static int IntOrDefault(this string v, int defaultValue) =>
            (int.TryParse(v, out var result)) ? result : defaultValue;

        public static TEnum EnumOrDefault<TEnum>(this string v, TEnum defaultValue) where TEnum : struct =>
            (Enum.TryParse<TEnum>(v, out var result)) ? result : defaultValue;

    }

    public static partial class Extensions
    {
        public static string EventStorageConnectionString(this IConfiguration _this) =>
            _this["ENV_EVENT_SOURCING_AZURE_STORAGE"]
                .StringOrDefault(
                    _this["ENV_DEFAULT_EVENT_SOURCING_AZURE_STORAGE"]
                    .StringOrDefault("UseDevelopmentStorage=true"));

        public static string EventStorageTableName(this IConfiguration _this) =>
            _this["ENV_EVENT_SOURCING_TABLE_NAME"]
                .StringOrDefault("events");

        public static CloudTable EventsTable(this IConfiguration _this)
        {
            var storageConnectionString = _this.EventStorageConnectionString();
            var cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
            var cloudTable = cloudTableClient.GetTableReference(_this.EventStorageTableName());
            cloudTable.CreateIfNotExists();
            return cloudTable;
        }
    }

    public static partial class Extensions
    {
#if CSHARP_8_AND_NETCORE_3
        public static async IAsyncEnumerable<T> GetAll<T>(this CloudTable table, TableQuery<T> query)
            where T : class, ITableEntity, new()
        {
            TableContinuationToken token = null;
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                foreach (var item in queryResult.Results)
                {
                    yield return item;
                }
                token = queryResult.ContinuationToken;
            } while (token != null);
        }
#else
        public static async Task<IEnumerable<T>> GetAll<T>(this CloudTable table, TableQuery<T> query)
            where T : class, ITableEntity, new()
        {
            List<T> result = new List<T>();

            TableContinuationToken token = null;
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, token);
                result.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return result;
        }
#endif
    }
}
