using Microsoft.Azure.Cosmos.Table;
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
