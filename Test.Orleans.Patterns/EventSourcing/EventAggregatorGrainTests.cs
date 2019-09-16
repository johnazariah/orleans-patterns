using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans.Patterns.EventSourcing;
using Orleans.Testing.Utilities;
using Orleans.TestingHost;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test.Orleans.Patterns.EventSourcing
{
    public interface ISummingGrain : IEventAggregatorGrain { }

    public class SummingGrain : EventAggregatorGrain<int>, ISummingGrain
    {
        public SummingGrain(IConfiguration configuration, ILogger<EventAggregatorGrain<int>> logger) : base(configuration, logger)
        { }

        protected override Func<(Guid, DateTimeOffset, int)> InitializeSeed(int seed) =>
            () => (Guid.Empty, DateTimeOffset.MinValue, seed);

        protected override (Guid, DateTimeOffset, int) ProcessEvent((Guid, DateTimeOffset, int) seed, BusinessEvent curr)
        {
            var (seedId, seedTimestamp, seedPayload) = seed;

            var (id, timestamp) =
                (curr.EventTimestamp > seedTimestamp)
                ? (curr.EventIdentifier, curr.EventTimestamp)
                : (seedId, seedTimestamp);

            switch (curr.BusinessEventEnum)
            {
                default:
                case 0:
                    {
                        var currPayload = curr.GetValue<int>();
                        var value = seedPayload + currPayload;

                        return (id, timestamp, value);
                    }
            }
        }
    }

    [Collection(ClusterCollection.Name)]
    public class EventAggregatorGrainTests
    {
        private readonly TestCluster _cluster;
        public EventAggregatorGrainTests(ClusterFixture fixture) =>
            _cluster = fixture?.Cluster ?? throw new ArgumentNullException(nameof(fixture));

        [Fact]
        public async Task Aggregator_Added_Before_Events_Recorded()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);
            var sum = await g.RegisterAggregateGrain<ISummingGrain>().ConfigureAwait(false);

            var expected = 0;
            for (var i = 0; i < 100; i++)
            {
                expected += i;
                await g.RecordEventPayload(0, i).ConfigureAwait(false);
            }

            var actual = await sum.GetValue<int>().ConfigureAwait(false);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Aggregator_Added_After_Events_Recorded()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            var expected = 0;
            for (var i = 0; i < 100; i++)
            {
                expected += i;
                await g.RecordEventPayload(0, i).ConfigureAwait(false);
            }

            var sum = await g.RegisterAggregateGrain<ISummingGrain>().ConfigureAwait(false);

            var actual = await sum.GetValue<int>().ConfigureAwait(false);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Aggregator_Can_Aggregate_Events_Incrementally_One_At_A_Time()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);
            var sum = await g.RegisterAggregateGrain<ISummingGrain>().ConfigureAwait(false);

            var expected = 0;
            for (var i = 0; i < 100; i++)
            {
                expected += i;
                Thread.Sleep(10);
                await g.RecordEventPayload(0, i).ConfigureAwait(false);
                var actual = await sum.GetValue<int>().ConfigureAwait(false);
                Assert.Equal(expected, actual);
            }
        }
    }
}
