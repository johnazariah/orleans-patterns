using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace Orleans.Patterns.EventSourcing
{
    public class EventSourcedGrain : Grain<EventSourcingContext>, IEventSourcedGrain
    {
        private readonly ILogger Logger;
        public EventSourcedGrain(CloudTable eventsTable, ILogger<EventSourcedGrain> logger)
        {
            EventsTable = eventsTable;
            Logger = logger;
        }

        public CloudTable EventsTable { get; }

        public Task<CloudTable> GetEventsTable() => Task.FromResult(EventsTable);

        public async Task<List<BusinessEvent>> GetEvents(DateTime? lastEventRaised = null)
        {
            var (events, _) = await EventsTable.FoldEventsAsync(
                this.GetPrimaryKey(),
                (result, curr) => { result.Add(curr); return result; },
                new List<BusinessEvent>(),
                lastEventRaised);

            return events;
        }

        public async Task<TAggregateGrain> RegisterAggregateGrain<TAggregateGrain>()
            where TAggregateGrain : IEventAggregatorGrain
        {
            var aggregateGrain = GrainFactory.GetGrain<TAggregateGrain>(this.GetPrimaryKey());
            State.PeerGrains.Add(aggregateGrain);
            await WriteStateAsync();

            return aggregateGrain;
        }

        public async Task RecordEventPayload<T>(T payload)
        {
            var e = new BusinessEvent<T>(payload)
            {
                PartitionKey = this.GetPrimaryKey().ToString("D"),
            };

            var tableOperation = TableOperation.Insert(e);
            await EventsTable.ExecuteAsync(tableOperation);
        }
    }
}
