using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Orleans.Patterns.EventSourcing;
using System;
using Test.Orleans.Patterns.Contracts;

namespace Test.Orleans.Patterns.Grains
{
    public class ComplexAddingAggregatorGrain : EventAggregatorGrain<ComplexNumber>, IComplexAddingAggregatorGrain
    {
        public ComplexAddingAggregatorGrain(CloudTable eventsTable, ILogger<ComplexAddingAggregatorGrain> logger) : base(eventsTable, logger) { }

        protected override (Guid, DateTime, ComplexNumber) ProcessEvent((Guid, DateTime, ComplexNumber) seed, BusinessEvent curr)
        {
            var (seedId, seedTimestamp, seedPayload) = seed;

            var (id, timestamp) =
                (curr.EventRaised > seedTimestamp)
                ? (curr.EventIdentifier, curr.EventRaised)
                : (seedId, seedTimestamp);

            var currPayload = curr.GetValue<ComplexNumber>();
            var value = new ComplexNumber(
                (seedPayload?.RealComponent ?? 0) + (currPayload?.RealComponent ?? 0),
                (seedPayload?.ImaginaryComponent ?? 0) + (currPayload?.ImaginaryComponent ?? 0));

            return (id, timestamp, value);
        }
    }
}
