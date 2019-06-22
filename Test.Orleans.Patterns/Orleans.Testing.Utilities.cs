using System;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Patterns.EventSourcing;
using Orleans.TestingHost;
using Xunit;

namespace Orleans.Testing.Utilities
{
    public class ClusterFixture : IDisposable
    {
        public ClusterFixture()
        {
            var builder = new TestClusterBuilder();
            builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
            Cluster = builder.Build();
            Cluster.Deploy();
        }

        public void Dispose()
        {
            Cluster.StopAllSilos();
        }

        public TestCluster Cluster { get; private set; }
    }

    public class TestSiloConfigurations : ISiloBuilderConfigurator
    {
        public IConfiguration Configuration { get; private set; }

        public void Configure(ISiloHostBuilder hostBuilder)
        {
            Configuration = hostBuilder.GetConfiguration();

            hostBuilder
            .AddMemoryGrainStorageAsDefault()
            .UseLocalhostClustering()
            .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory())
            .ConfigureServices(services =>
            {
                services.AddSingleton(CloudTableFactory(Configuration));
            });
        }

        private static Func<IServiceProvider, CloudTable> CloudTableFactory(IConfiguration configuration) =>
            _ =>
            {
                var storageConnectionString = configuration.EventStorageConnectionString();
                var cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
                var cloudTableClient = cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
                var cloudTable = cloudTableClient.GetTableReference(configuration.EventStorageTableName());
                cloudTable.CreateIfNotExists();
                return cloudTable;
            };
    }

    [CollectionDefinition(Name)]
    public class ClusterCollection : ICollectionFixture<ClusterFixture>
    {
        public const string Name = nameof(ClusterCollection);
    }
}

// namespace Tests
// {
//     public class HelloGrainTests
//     {
//         [Fact]
//         public async Task SaysHelloCorrectly()
//         {
//             var cluster = new TestCluster();
//             cluster.Deploy();

//             var hello = cluster.GrainFactory.GetGrain<IHelloGrain>(Guid.NewGuid());
//             var greeting = await hello.SayHello();

//             cluster.StopAllSilos();

//             Assert.Equal("Hello, World", greeting);
//         }
//     }
// }

// namespace Tests
// {
//     [Collection(ClusterCollection.Name)]
//     public class HelloGrainTests
//     {
//         private readonly TestCluster _cluster;

//         public HelloGrainTests(ClusterFixture fixture)
//         {
//             _cluster = fixture.Cluster;
//         }

//         [Fact]
//         public async Task SaysHelloCorrectly()
//         {
//             var hello = _cluster.GrainFactory.GetGrain<IHelloGrain>(Guid.NewGuid());
//             var greeting = await hello.SayHell();

//             Assert.Equal("Hello, World", greeting);
//         }
//     }
// }
