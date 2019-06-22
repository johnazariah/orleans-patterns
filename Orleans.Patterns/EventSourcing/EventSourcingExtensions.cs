using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace Orleans.Patterns.EventSourcing
{
    internal static partial class EventSourcingExtensions
    {
        public async static Task<(TResult, List<(BusinessEvent, Exception)>)> FoldEventsAsync<TResult>(
            this CloudTable EventsTable,
            Guid primaryKey,
            Func<TResult, BusinessEvent, TResult> accumulator,
            TResult seed = default,
            DateTime? lastEventRaised = null,
            ILogger logger = null)
        {
            DateTimeOffset cutOff = lastEventRaised ?? new DateTime(1601, 1, 1);

            logger?.LogInformation("EventSourcedGrain.FoldEventsAsync :: Filtering for events after {EventsAfter}", cutOff);

            var queryFilter =
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(
                        "PartitionKey", QueryComparisons.Equal, primaryKey.ToString("D")),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForDate(
                        "EventRaised", QueryComparisons.GreaterThanOrEqual, cutOff));

            logger?.LogInformation("EventSourcedGrain.FoldEventsAsync :: Filter query {FilterQuery}", queryFilter);

            var query = new TableQuery<BusinessEvent>().Where(queryFilter);

            var failures = new List<(BusinessEvent, Exception)>();
            var result = seed;

            TableContinuationToken token = null;
            do
            {
                var queryResult = await EventsTable.ExecuteQuerySegmentedAsync(query, token);

                foreach (var e in queryResult.Results)
                {
                    try
                    {
                        result = accumulator(result, e);
                    }
                    catch (Exception ex)
                    {
                        failures.Add((e, ex));
                    }
                }

                token = queryResult.ContinuationToken;
            } while (token != null);

            logger?.LogInformation("EventSourcedGrain.FoldEventsAsync :: Failure Count {FailureCount}", failures.Count);

            return (result, failures);
        }
    }
}
