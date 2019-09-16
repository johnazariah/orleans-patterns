using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Patterns.EventSourcing
{
    public class EventSourcedTableGrain<TRowGrain> : EventSourcedRegistryGrain<TRowGrain>, IEventSourcedTableGrain<TRowGrain>
        where TRowGrain : IEventSourcedRowGrain
    {
        public EventSourcedTableGrain(IConfiguration configuration, ILogger<EventSourcedTableGrain<TRowGrain>> logger)
            : base(configuration, logger)
        { }

        public async Task<TRowGrain> CreateRow(Guid id)
        {
            Logger.LogTrace($"EventSourcedTableGrain.CreateRow({id}) :: Entry");
            var rowGrain = GrainFactory.GetGrain<TRowGrain>(id);
            Logger.LogTrace($"EventSourcedTableGrain.CreateRow({id}) :: GrainFactory.GetGrain returned");
            await RegisterGrain(rowGrain);
            Logger.LogTrace($"EventSourcedTableGrain.CreateRow({id}) :: Grain Registered");

            Logger.LogTrace($"EventSourcedTableGrain.CreateRow({id}) :: Success");
            return rowGrain;
        }

        public async Task DeleteRow(TRowGrain rowGrain)
        {
            Logger.LogTrace($"EventSourcedTableGrain.DeleteRow() :: Entry");
            var id = rowGrain.GetPrimaryKey();
            Logger.LogTrace($"EventSourcedTableGrain.DeleteRow({id}) :: Grain selected for deletion");
            await rowGrain.MarkDeleted();
            Logger.LogTrace($"EventSourcedTableGrain.DeleteRow({id}) :: Grain marked for deletion");
            await UnregisterGrain(rowGrain);
            Logger.LogTrace($"EventSourcedTableGrain.DeleteRow({id}) :: Grain Unregistered");

            Logger.LogTrace($"EventSourcedTableGrain.CreateRow({id}) :: Success");
        }

        public Task<HashSet<Guid>> GetRows() => GetMemberGrains();
    }
}
