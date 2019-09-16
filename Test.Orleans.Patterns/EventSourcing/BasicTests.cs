using System;
using System.Threading.Tasks;
using Orleans.Testing.Utilities;
using Orleans.TestingHost;
using Orleans.Patterns.EventSourcing;
using Xunit;
using Test.Orleans.Patterns.Contracts;

namespace Test.Orleans.Patterns.EventSourcing
{
    public enum NumberOperation { Add = 0, MAX = 1 };
    public enum ComplexNumberOperation { Add = NumberOperation.MAX + 1000 };

    [Collection(ClusterCollection.Name)]
    public class BasicTests
    {
        private readonly TestCluster _cluster;
        public BasicTests (ClusterFixture fixture) =>
            _cluster = fixture?.Cluster ?? throw new ArgumentNullException(nameof(fixture));

        [Fact]
        public async Task New_Version_Aggregator_Processes_Old_Version_Events__Upgrade_Scenario()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            for (var i = 1; i <= 100; i++)
            {
                await g.RecordEventPayload((int)NumberOperation.Add, new Number(i)).ConfigureAwait(false);
            }

            var firstVersionAdder = await g.RegisterAggregateGrain<IAddingAggregatorGrain>().ConfigureAwait(false);
            var firstVersionResult = await firstVersionAdder.GetValue<Number>().ConfigureAwait(false);

            var nextVersionAdder = await g.RegisterAggregateGrain<IComplexAddingAggregatorGrain>().ConfigureAwait(false);
            var nextVersionResult = await nextVersionAdder.GetValue<ComplexNumber>().ConfigureAwait(false);

            Assert.Equal(5050, firstVersionResult.Value);
            Assert.Equal(firstVersionResult.Value, nextVersionResult.RealComponent);
            Assert.Equal(0, nextVersionResult.ImaginaryComponent);
        }

        [Fact]
        public async Task Old_Version_Aggregator_Processes_New_Version_Events__Rollback_Scenario()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            for (var i = 1; i <= 100; i++)
            {
                await g.RecordEventPayload((int)ComplexNumberOperation.Add, new ComplexNumber(i, 0.0)).ConfigureAwait(false);
            }

            var nextVersionAdder = await g.RegisterAggregateGrain<IComplexAddingAggregatorGrain>().ConfigureAwait(false);
            var nextVersionResult = await nextVersionAdder.GetValue<ComplexNumber>().ConfigureAwait(false);

            var firstVersionAdder = await g.RegisterAggregateGrain<IAddingAggregatorGrain>().ConfigureAwait(false);
            var firstVersionResult = await firstVersionAdder.GetValue<Number>().ConfigureAwait(false);

            Assert.Equal(5050, nextVersionResult.RealComponent);
            Assert.Equal(0, nextVersionResult.ImaginaryComponent);
            Assert.Equal(firstVersionResult.Value, nextVersionResult.RealComponent);
        }
    }
}
