using System;
using System.Threading.Tasks;
using Orleans.Testing.Utilities;
using Orleans.TestingHost;
using Orleans.Patterns.EventSourcing;
using Xunit;
using System.Linq;
using Test.Orleans.Patterns.Contracts;

namespace Test.Orleans.Patterns.EventSourcing
{
    [Collection(ClusterCollection.Name)]
    public class BasicTests
    {
        private readonly TestCluster _cluster;
        public BasicTests (ClusterFixture fixture) => _cluster = fixture.Cluster;

        [Fact]
        public async Task CanSuccessfullyRegisterGrain()
        {
            var payload = new Number(4);
            var grainId = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(grainId);
            await g.RecordEventPayload(payload);

            var rawBusinessEvent = (await g.GetEvents()).First();

            var businessEvent = BusinessEvent<Number>.Read(rawBusinessEvent);

            Assert.Equal(payload.Value, businessEvent.Payload.Value);
        }

        [Fact]
        public async Task LiveAddingTheFirstHundredNumbers()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            var adder = await g.RegisterAggregateGrain<IAddingAggregatorGrain>();

            for (var i = 1; i <= 100; i++)
            {
                await g.RecordEventPayload(new Number(i));
            }

            var result = await adder.GetValue<Number>();

            Assert.Equal(5050, result.Value);
        }

        [Fact]
        public async Task AddingTheFirstHundredNumbersAfterTheFact()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            for (var i = 1; i <= 100; i++)
            {
                await g.RecordEventPayload(new Number(i));
            }

            var adder = await g.RegisterAggregateGrain<IAddingAggregatorGrain>();

            var result = await adder.GetValue<Number>();

            Assert.Equal(5050, result.Value);
        }

        [Fact]
        public async Task SavingOldVersionAndReadingNewVersion()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            for (var i = 1; i <= 100; i++)
            {
                await g.RecordEventPayload(new Number(i));
            }

            var firstVersionAdder = await g.RegisterAggregateGrain<IAddingAggregatorGrain>();
            var firstVersionResult = await firstVersionAdder.GetValue<Number>();

            var nextVersionAdder = await g.RegisterAggregateGrain<IComplexAddingAggregatorGrain>();
            var nextVersionResult = await nextVersionAdder.GetValue<ComplexNumber>();

            Assert.Equal(5050, firstVersionResult.Value);
            Assert.Equal(firstVersionResult.Value, nextVersionResult.RealComponent);
            Assert.Equal(0, nextVersionResult.ImaginaryComponent);
        }

        [Fact]
        public async Task SavingNewVersionAndReadingOldVersion()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            for (var i = 1; i <= 100; i++)
            {
                await g.RecordEventPayload(new ComplexNumber(i, 0.0));
            }

            var nextVersionAdder = await g.RegisterAggregateGrain<IComplexAddingAggregatorGrain>();
            var nextVersionResult = await nextVersionAdder.GetValue<ComplexNumber>();

            var firstVersionAdder = await g.RegisterAggregateGrain<IAddingAggregatorGrain>();
            var firstVersionResult = await firstVersionAdder.GetValue<Number>();

            Assert.Equal(5050, nextVersionResult.RealComponent);
            Assert.Equal(0, nextVersionResult.ImaginaryComponent);
            Assert.Equal(firstVersionResult.Value, nextVersionResult.RealComponent);
        }
    }
}
