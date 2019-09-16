using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Patterns.EventSourcing
{
    [Serializable]
    public class RegistryState<TPrimaryKey>
    {
        public HashSet<TPrimaryKey> MemberGrains { get; set; } = new HashSet<TPrimaryKey>();
    }

    internal enum RegistryOperations { RegisterGrain = 0, UnregisterGrain }

    public interface IEventSourcedRegistryAggregator : IEventAggregatorGrain { }

    public class EventSourcedRegistryAggregator : EventAggregatorGrain<RegistryState<Guid>>, IEventSourcedRegistryAggregator
    {
        public EventSourcedRegistryAggregator(IConfiguration configuration, ILogger<EventSourcedRegistryAggregator> logger)
            : base(configuration, logger) { }

        protected override Func<(Guid, DateTimeOffset, RegistryState<Guid>)> InitializeSeed(RegistryState<Guid> seed) =>
            () => (Guid.Empty, DateTimeOffset.MinValue, seed ?? new RegistryState<Guid>());

        protected override (Guid, DateTimeOffset, RegistryState<Guid>) ProcessEvent((Guid, DateTimeOffset, RegistryState<Guid>) seed, BusinessEvent curr)
        {
            var (seedId, seedTimestamp, seedPayload) = seed;

            var (id, timestamp) =
                (curr.EventTimestamp > seedTimestamp)
                ? (curr.EventIdentifier, curr.EventTimestamp)
                : (seedId, seedTimestamp);

            Logger.LogTrace($"EventSourcedRegistryAggregator.ProcessEvent :: Processing Event {id}");

            switch (curr.BusinessEventEnum)
            {
                default:
                case (int)RegistryOperations.RegisterGrain:
                    {
                        var currPayload = curr.GetValue<Guid>();
                        Logger.LogTrace($"EventSourcedRegistryAggregator.ProcessEvent :: Registering {currPayload}");
                        seedPayload?.MemberGrains.Add(currPayload);
                        return (id, timestamp, seedPayload);
                    }

                case (int)RegistryOperations.UnregisterGrain:
                    {
                        var currPayload = curr.GetValue<Guid>();
                        Logger.LogTrace($"EventSourcedRegistryAggregator.ProcessEvent :: Unregistering {currPayload}");
                        seedPayload?.MemberGrains.Remove(currPayload);
                        return (id, timestamp, seedPayload);
                    }
            }
        }
    }

    public class EventSourcedRegistryGrain<TGrain> : EventSourcedGrain, IEventSourcedRegistryGrain<TGrain>
        where TGrain : IEventSourcedRowGrain
    {
        public EventSourcedRegistryGrain(IConfiguration configuration, ILogger<EventSourcedRegistryGrain<TGrain>> logger)
            : base(configuration, logger)
        { }

        public override async Task OnActivateAsync()
        {
            Logger.LogTrace($"EventSourcedRegistryGrain.OnActivateAsync :: Entry");

            var id = this.GetPrimaryKey();
            Logger.LogTrace($"EventSourcedRegistryGrain.OnActivateAsync :: Primary Key : {id}");

            RegistryStateAggregator = await RegisterAggregateGrain<IEventSourcedRegistryAggregator>();
            Logger.LogTrace($"EventSourcedRegistryGrain.OnActivateAsync :: Obtained and set RegistryStateAggregator synchronously");

            Logger.LogTrace($"EventSourcedRegistryGrain.OnActivateAsync :: Success");
        }

        protected IEventSourcedRegistryAggregator RegistryStateAggregator { get; private set; }

        public async Task RegisterGrain(TGrain candidateGrain)
        {
            var id = candidateGrain.GetPrimaryKey();

            Logger.LogTrace($"EventSourcedRegistryGrain.RegisterGrain({id}) :: Entry");
            await RecordEventPayload((int)RegistryOperations.RegisterGrain, id);

            Logger.LogTrace($"EventSourcedRegistryGrain.RegisterGrain({id}) :: Success");
        }

        public async Task UnregisterGrain(TGrain candidateGrain)
        {
            var id = candidateGrain.GetPrimaryKey();

            Logger.LogTrace($"EventSourcedRegistryGrain.UnregisterGrain({id}) :: Entry");
            await RecordEventPayload((int)RegistryOperations.UnregisterGrain, id);

            Logger.LogTrace($"EventSourcedRegistryGrain.UnregisterGrain({id}) :: Success");
        }

        public async Task<HashSet<Guid>> GetMemberGrains() =>
            (await RegistryStateAggregator.GetValue<RegistryState<Guid>>())?.MemberGrains;
    }
}
