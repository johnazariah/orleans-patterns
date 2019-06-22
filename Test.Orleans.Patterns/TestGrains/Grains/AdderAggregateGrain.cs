using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Orleans.Patterns.EventSourcing;
using System;
using Test.Orleans.Patterns.Contracts;

namespace Test.Orleans.Patterns.Grains
{

    public class AddingAggregatorGrain : EventAggregatorGrain<Number>, IAddingAggregatorGrain
    {
        public AddingAggregatorGrain(CloudTable eventsTable, ILogger<AddingAggregatorGrain> logger) : base(eventsTable, logger) { }

        protected override (Guid, DateTime, Number) ProcessEvent((Guid, DateTime, Number) seed, BusinessEvent curr)
        {
            var (seedId, seedTimestamp, seedPayload) = seed;

            var (id, timestamp) =
                (curr.EventRaised > seedTimestamp)
                ? (curr.EventIdentifier, curr.EventRaised)
                : (seedId, seedTimestamp);

            var currPayload = curr.GetValue<Number>();
            var value = new Number((seedPayload?.Value ?? 0) + (currPayload?.Value ?? 0));

            return (id, timestamp, value);
        }
    }
}
