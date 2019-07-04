using Microsoft.Extensions.Configuration;
using Orleans.Patterns.Utilities;

namespace Test.Orleans.Patterns.EventSourcing
{
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
    }
}