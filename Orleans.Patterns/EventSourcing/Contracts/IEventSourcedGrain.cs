using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Patterns.EventSourcing
{
    public interface IEventSourcedGrain : IGrainWithGuidKey
    {
        Task<CloudTable> GetEventsTable();
        Task RecordEventPayload<T>(T e);
        Task<TAggregateGrain> RegisterAggregateGrain<TAggregateGrain>() where TAggregateGrain : IEventAggregatorGrain;
        Task<List<BusinessEvent>> GetEvents(DateTime? lastEventRaised = null);
    }
}
