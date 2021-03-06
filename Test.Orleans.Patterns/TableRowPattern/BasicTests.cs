using System;
using System.Threading.Tasks;
using Orleans.Testing.Utilities;
using Orleans.TestingHost;
using Xunit;
using Test.Orleans.Patterns.Contracts;
using System.Linq;

namespace Test.Orleans.Patterns.TableRowPattern
{
    [Collection(ClusterCollection.Name)]
    public class BasicTests
    {
        private static readonly Guid TestTableId = Guid.Parse("7c38d458-2597-48c5-8b2f-5f78b46df049");

        private readonly TestCluster _cluster;
        public BasicTests (ClusterFixture fixture) =>
            _cluster = fixture?.Cluster ?? throw new ArgumentNullException(nameof(fixture));

        [Fact]
        public async Task CanSuccessfullyCreateARow()
        {
            var testRowId = Guid.NewGuid();
            var testRowState = new TestRowState { Id = testRowId, Value = 3 };

            var table = _cluster.GrainFactory.GetGrain<ITestRowTableGrain>(TestTableId);
            var row = await table.CreateRow(testRowId, testRowState).ConfigureAwait(false);

            var rowRs = await table.GetRows().ConfigureAwait(false);
            var rowR = rowRs.Single();

            var (actualId, actualValue) = (rowR.Id, rowR.Value);

            Assert.Equal(testRowId, actualId);
            Assert.Equal(testRowState.Value, actualValue);
        }
    }
}
