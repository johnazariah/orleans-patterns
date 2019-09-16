using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans.Patterns.EventSourcing;
using Orleans.Testing.Utilities;
using Orleans.TestingHost;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Test.Orleans.Patterns.EventSourcing
{
    public interface ITestRowGrain : IEventSourcedRowGrain { }
    public class TestRowGrain : EventSourcedRowGrain, ITestRowGrain
    {
        public TestRowGrain(IConfiguration configuration, ILogger<TestRowGrain> logger) : base(configuration, logger)
        { }
    }

    [Collection(ClusterCollection.Name)]
    public class EventSourcedRowGrainTests
    {
        private readonly TestCluster _cluster;
        public EventSourcedRowGrainTests(ClusterFixture fixture) =>
            _cluster = fixture?.Cluster ?? throw new ArgumentNullException(nameof(fixture));

        [Fact]
        public async Task GetIdentity_Returns_Correct_Identity()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<ITestRowGrain>(primaryKey);
            var actual = await g.GetIdentity();
            Assert.Equal(primaryKey, actual);
        }

        [Fact]
        public async Task GetJsonDocument_Returns_Correct_JsonDocument()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<ITestRowGrain>(primaryKey);

            var expected = "{ \"test\" : 42 }";
            await g.SetRowJson(expected);
            var actual = await g.GetRowJson();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task MarkDeleted_Marks_Row_As_Deleted ()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<ITestRowGrain>(primaryKey);

            Assert.False(await g.IsDeleted());

            await g.MarkDeleted();
            Assert.True(await g.IsDeleted());
        }

        [Fact]
        public async Task MarkUndeleted_Marks_Row_As_Undeleted()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<ITestRowGrain>(primaryKey);

            Assert.False(await g.IsDeleted());

            await g.MarkDeleted();
            Assert.True(await g.IsDeleted());

            await g.MarkUndeleted();
            Assert.False(await g.IsDeleted());
        }
    }
}
