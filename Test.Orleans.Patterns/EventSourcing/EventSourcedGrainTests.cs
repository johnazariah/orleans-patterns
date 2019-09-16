using Orleans.Patterns.EventSourcing;
using Orleans.Testing.Utilities;
using Orleans.TestingHost;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Test.Orleans.Patterns.EventSourcing
{
    [Collection(ClusterCollection.Name)]
    public class EventSourcedGrainTests
    {
        private readonly TestCluster _cluster;
        public EventSourcedGrainTests(ClusterFixture fixture) =>
            _cluster = fixture?.Cluster ?? throw new ArgumentNullException(nameof(fixture));

        [Fact]
        public async Task Recorded_Events_Can_Be_Accessed_From_EventSourcedGrain()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);
            await g.RecordEventPayload(0, 4).ConfigureAwait(false);

            var rawBusinessEvent = (await g.GetEvents().ConfigureAwait(false)).First();

            var businessEvent = BusinessEvent<int>.Read(rawBusinessEvent);

            Assert.Equal(4, businessEvent.Payload);
        }

        [Fact]
        public async Task Recorded_Events_Can_Be_Retrieved()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            for (var i = 0; i < 100; i++)
            {
                await g.RecordEventPayload(0, i).ConfigureAwait(false);
            }

            var recordedEvents = await g.GetEvents().ConfigureAwait(false);

            var sortedEvents = recordedEvents.OrderBy(bu => bu.EventTimestamp).ToList();
            Assert.Equal(100, sortedEvents.Count);

            for (var i = 0; i < 100; i++)
            {
                Assert.Equal(0, sortedEvents[i].BusinessEventEnum);
                Assert.Equal(i, sortedEvents[i].GetValue<int>());
            }
        }

        [Fact]
        public async Task Recorded_Events_Are_Retrieved_In_Sorted_Order()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            for (var i = 0; i < 100; i++)
            {
                await g.RecordEventPayload(0, i).ConfigureAwait(false);
            }

            var recordedEvents = await g.GetEvents().ConfigureAwait(false);
            var sortedEvents = recordedEvents.OrderBy(bu => bu.EventTimestamp).ToList();
            Assert.Equal(100, recordedEvents.Count);

            for (var i = 0; i < 100; i++)
            {
                Assert.Equal(sortedEvents[i].EventIdentifier, recordedEvents[i].EventIdentifier);
            }
        }

        [Fact]
        public async Task Retrieved_Events_Honour_Cutoff()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            DateTimeOffset? lastEventTimestamp = null;
            for (var i = 0; i < 100; i++)
            {
                var e = await g.RecordEventPayload(0, i).ConfigureAwait(false);
                var recordedEvents = await g.GetEvents(lastEventTimestamp).ConfigureAwait(false);

                lastEventTimestamp = e.EventTimestamp;

                // there should be only one event recorded since the last timestamp...
                Assert.Single(recordedEvents);

                //...and that event should be the one we just recorded
                Assert.Equal(e.EventIdentifier, recordedEvents[0].EventIdentifier);
                Assert.Equal(e.EventTimestamp, recordedEvents[0].EventTimestamp);
                Assert.Equal(e.PayloadJson, recordedEvents[0].PayloadJson);
                Assert.Equal(e.PayloadType, recordedEvents[0].PayloadType);
            }

            // we should have actually recorded all events though
            var allEvents = await g.GetEvents().ConfigureAwait(false);
            Assert.Equal(100, allEvents.Count);
        }
    }
}
