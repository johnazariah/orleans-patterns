using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Patterns.EventSourcing
{
    public interface IEventSourcedGrain : IGrainWithGuidKey
    {
        Task<BusinessEvent> RecordEventPayload<T>(int businessEventEnum, T e);
        Task<TAggregateGrain> RegisterAggregateGrain<TAggregateGrain>() where TAggregateGrain : IEventAggregatorGrain;
        Task<List<BusinessEvent>> GetEvents(DateTimeOffset? lastEventRaised = null);
    }
}
