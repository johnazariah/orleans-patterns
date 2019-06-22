using System;
using System.Collections.Generic;

namespace Orleans.Patterns.EventSourcing
{
    [Serializable]
    public class EventSourcingContext
    {
        public List<IEventAggregatorGrain> PeerGrains { get; } = new List<IEventAggregatorGrain>();
    }
}