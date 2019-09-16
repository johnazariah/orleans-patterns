using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans.Patterns.EventSourcing;
using Orleans.Testing.Utilities;
using Orleans.TestingHost;
using System;
using System.Linq;
using System.Threading.Tasks;
using Test.Orleans.Patterns.Contracts;
using Xunit;

namespace Test.Orleans.Patterns.EventSourcing
{
    public interface ITestTableGrain : IEventSourcedTableGrain<ITestRowGrain> { }
    public class TestTableGrain : EventSourcedTableGrain<ITestRowGrain>, ITestTableGrain
    {
        public TestTableGrain(IConfiguration configuration, ILogger<TestTableGrain> logger)
            : base(configuration, logger)
        { }
    }

    [Collection(ClusterCollection.Name)]
    public class EventSourcedTableGrainTests
    {
        private readonly TestCluster _cluster;
        public EventSourcedTableGrainTests(ClusterFixture fixture) =>
            _cluster = fixture?.Cluster ?? throw new ArgumentNullException(nameof(fixture));


        [Fact]
        public async Task TableGrain_Creates_And_Registers_RowGrains()
        {
            var tableGrain = _cluster.GrainFactory.GetGrain<ITestTableGrain>(Guid.NewGuid());

            var expected = Guid.NewGuid();
            var rowGrain = await tableGrain.CreateRow(expected);
            Assert.NotNull(rowGrain);

            var memberGrains = await tableGrain.GetMemberGrains();
            var actual = memberGrains.Single();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task TableGrain_GetRows_And_GetMemberGrains_Are_Identical()
        {
            var tableGrain = _cluster.GrainFactory.GetGrain<ITestTableGrain>(Guid.NewGuid());

            var expected = Guid.NewGuid();
            var rowGrain = await tableGrain.CreateRow(expected);
            Assert.NotNull(rowGrain);

            Assert.Equal<Guid>(await tableGrain.GetRows(), await tableGrain.GetMemberGrains());
        }

        [Fact]
        public async Task TableGrain_Deletes_And_Unregisters_RowGrains()
        {
            var tableGrain = _cluster.GrainFactory.GetGrain<ITestTableGrain>(Guid.NewGuid());

            var expected = Guid.NewGuid();
            var rowGrain = await tableGrain.CreateRow(expected);
            Assert.NotNull(rowGrain);

            {
                var memberGrains = await tableGrain.GetMemberGrains();
                var actual = memberGrains.Single();
                Assert.Equal(expected, actual);
            }

            await tableGrain.DeleteRow(rowGrain);
            {
                var memberGrains = await tableGrain.GetMemberGrains();
                Assert.Empty(memberGrains);
            }
        }

        [Fact]
        public async Task Singleton_Factory_Always_Returns_The_Same_Table_Grain()
        {
            var tableGrainPre = SingletonGrainFactory.GetSingletonGrain<ITestTableGrain>(_cluster.GrainFactory);
            var expected = Guid.NewGuid();
            var rowGrain = await tableGrainPre.CreateRow(expected);
            Assert.NotNull(rowGrain);

            var tableGrainPost = SingletonGrainFactory.GetSingletonGrain<ITestTableGrain>(_cluster.GrainFactory);
            Assert.Equal<Guid>(await tableGrainPre.GetRows(), await tableGrainPost.GetRows());
            Assert.Equal<Guid>(await tableGrainPre.GetMemberGrains(), await tableGrainPost.GetMemberGrains());
        }
    }
}
