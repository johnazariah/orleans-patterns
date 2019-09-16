using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Orleans.Patterns.EventSourcing;
using System;
using System.Diagnostics.Contracts;
using Test.Orleans.Patterns.Contracts;
using Test.Orleans.Patterns.EventSourcing;

namespace Test.Orleans.Patterns.Grains
{
    public class ComplexAddingAggregatorGrain : EventAggregatorGrain<ComplexNumber>, IComplexAddingAggregatorGrain
    {
        public ComplexAddingAggregatorGrain(CloudTable eventsTable, ILogger<ComplexAddingAggregatorGrain> logger) : base(eventsTable, logger) { }

        protected override Func<(Guid, DateTimeOffset, ComplexNumber)> InitializeSeed(ComplexNumber seed) =>
            () => (Guid.Empty, DateTimeOffset.MinValue, seed ?? new ComplexNumber(0.0, 0.0));

        protected override (Guid, DateTimeOffset, ComplexNumber) ProcessEvent((Guid, DateTimeOffset, ComplexNumber) seed, BusinessEvent curr)
        {
            Contract.Requires(curr != null);

            var (seedId, seedTimestamp, seedPayload) = seed;

            var (id, timestamp) =
                (curr.EventTimestamp > seedTimestamp)
                ? (curr.EventIdentifier, curr.EventTimestamp)
                : (seedId, seedTimestamp);

            switch (curr.BusinessEventEnum)
            {
                case (int)NumberOperation.Add:
                    {
                        var currPayload = curr.GetValue<Number>();
                        var value = (seedPayload ?? ComplexNumber.Zero).Combine(currPayload);
                        return (id, timestamp, value);
                    }

                default:
                case (int)ComplexNumberOperation.Add:
                    {
                        var currPayload = curr.GetValue<ComplexNumber>();
                        var value = new ComplexNumber(
                            (seedPayload?.RealComponent ?? 0) + (currPayload?.RealComponent ?? 0),
                            (seedPayload?.ImaginaryComponent ?? 0) + (currPayload?.ImaginaryComponent ?? 0));
                        return (id, timestamp, value);
                    }
            }
        }
    }
}
