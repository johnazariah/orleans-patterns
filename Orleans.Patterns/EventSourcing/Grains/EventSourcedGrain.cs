using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans.Patterns.Utilities;

namespace Orleans.Patterns.EventSourcing
{
    public class EventSourcedGrain : Grain<EventSourcingContext>, IEventSourcedGrain
    {
        public EventSourcedGrain(IConfiguration configuration, ILogger<EventSourcedGrain> logger)
        {
            EventsTable = configuration.EventsTable();
            Logger = logger;
        }

        public CloudTable EventsTable { get; }

        protected ILogger Logger { get; }

        public async Task<List<BusinessEvent>> GetEvents(DateTimeOffset? lastEventRaised = null)
        {
            var (events, _) = await EventsTable.FoldEventsAsync(
                this.GetPrimaryKey(),
                (result, curr) => { result.Add(curr); return result; },
                () => new List<BusinessEvent>(),
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

        public async Task<BusinessEvent> RecordEventPayload<T>(int businessEventEnum, T payload)
        {
            var e = new BusinessEvent<T>(businessEventEnum, payload)
            {
                PartitionKey = this.GetPrimaryKey().ToString("D"),
            };

            var tableOperation = TableOperation.Insert(e);
            await EventsTable.ExecuteAsync(tableOperation);
            return e;
        }
    }
}
