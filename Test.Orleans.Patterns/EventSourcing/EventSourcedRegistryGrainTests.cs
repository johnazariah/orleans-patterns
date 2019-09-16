using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans.Patterns.EventSourcing;
using Orleans.Testing.Utilities;
using Orleans.TestingHost;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Test.Orleans.Patterns.EventSourcing
{
    public interface ITestRegistryGrain : IEventSourcedRegistryGrain<ITestRowGrain> { }
    public class TestRegistryGrain : EventSourcedRegistryGrain<ITestRowGrain>, ITestRegistryGrain
    {
        public TestRegistryGrain(IConfiguration configuration, ILogger<TestRegistryGrain> logger)
            : base(configuration, logger)
        { }
    }

    [Collection(ClusterCollection.Name)]
    public class EventSourcedRegistryGrainTests
    {
        private readonly TestCluster _cluster;
        public EventSourcedRegistryGrainTests(ClusterFixture fixture) =>
            _cluster = fixture?.Cluster ?? throw new ArgumentNullException(nameof(fixture));

        [Fact]
        public async Task RegistryGrain_Persistantly_Registers_RowGrains()
        {
            var expected = Guid.NewGuid();
            var rowGrain = _cluster.GrainFactory.GetGrain<ITestRowGrain>(expected);

            var registryId = Guid.NewGuid();
            var registryGrain = _cluster.GrainFactory.GetGrain<ITestRegistryGrain>(registryId);
            await registryGrain.RegisterGrain(rowGrain);

            var memberGrains = await registryGrain.GetMemberGrains();
            var actual = memberGrains.Single();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task RegistryGrain_Persistantly_Unregisters_RowGrains()
        {
            var expected = Guid.NewGuid();
            var rowGrain = _cluster.GrainFactory.GetGrain<ITestRowGrain>(expected);

            var registryId = Guid.NewGuid();
            var registryGrain = _cluster.GrainFactory.GetGrain<ITestRegistryGrain>(registryId);

            {
                await registryGrain.RegisterGrain(rowGrain);
                var memberGrains = await registryGrain.GetMemberGrains();
                var actual = memberGrains.Single();
                Assert.Equal(expected, actual);
            }

            {
                await registryGrain.UnregisterGrain(rowGrain);
                var memberGrains = await registryGrain.GetMemberGrains();
                Assert.Empty(memberGrains);
            }
        }
    }
}
