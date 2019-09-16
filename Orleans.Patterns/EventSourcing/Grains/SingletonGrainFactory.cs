using Microsoft.Extensions.Logging;
using System;

namespace Orleans.Patterns.EventSourcing
{
    public static class SingletonGrainFactory
    {
        public static readonly Guid SingletonGrainId = Guid.Parse("c324326d-2bc3-4ad0-86f5-000bcd6cf033");
        public static TGrain GetSingletonGrain<TGrain>(IGrainFactory grainFactory) where TGrain : IGrainWithGuidKey =>
            grainFactory.GetGrain<TGrain>(SingletonGrainId);
    }

#if NEVER
    public class EventSourcedRegistryProcessor<TGrain>
    {
        public EventSourcedRegistryProcessor(IEventSourcedRegistryGrain<TGrain> registryIEventSourcedRegistryAggregator registryStateAggregator, ILogger logger, IGrainFactory grainFactory)
        {
            RegistryStateAggregator = registryStateAggregator;
            Logger = logger;
            GrainFactory = grainFactory;
        }

        private IEventSourcedRegistryAggregator RegistryStateAggregator { get; }
        public ILogger Logger { get; }
        public IGrainFactory GrainFactory { get; }

        public async Task Iter(Action<TGrain> func)
        {
            Logger.LogTrace($"EventSourcedRegistryGrain.Iter() :: Entry");

            var state = await RegistryStateAggregator.GetValue<RegistryState>();
            Logger.LogTrace($"EventSourcedRegistryGrain.Iter() :: Obtained state");

            var results =
                from id in state.MemberGrains
                let grain = GrainFactory.GetGrain<TGrain>(id)
                select grain;

            Logger.LogTrace($"EventSourcedRegistryGrain.Iter() :: Obtained grains");
            foreach (var grain in results) { func(grain); }
            Logger.LogTrace($"EventSourcedRegistryGrain.Iter() :: Finished iteration over grains");

            Logger.LogTrace($"EventSourcedRegistryGrain.Iter() :: Success");
            return;
        }

        public async Task<List<TResult>> Map<TResult>(Func<TGrain, TResult> func)
        {
            Logger.LogTrace($"EventSourcedRegistryGrain.Map() :: Entry");

            var state = await RegistryStateAggregator.GetValue<RegistryState>();
            Logger.LogTrace($"EventSourcedRegistryGrain.Map() :: Obtained state");

            var results =
                from id in state.MemberGrains
                let grain = GrainFactory.GetGrain<TGrain>(id)
                select func(grain);
            Logger.LogTrace($"EventSourcedRegistryGrain.Map() :: Finished mapping over grains");

            Logger.LogTrace($"EventSourcedRegistryGrain.Map() :: Success");
            return new List<TResult>(results);
        }

        public async Task<List<TGrain>> Filter<TResult>(Func<TGrain, bool> predicate)
        {
            Logger.LogTrace($"EventSourcedRegistryGrain.Filter() :: Entry");

            var state = await RegistryStateAggregator.GetValue<RegistryState>();
            Logger.LogTrace($"EventSourcedRegistryGrain.Filter() :: Obtained state");

            var results =
                from id in state.MemberGrains
                let grain = GrainFactory.GetGrain<TGrain>(id)
                where predicate(grain)
                select grain;
            Logger.LogTrace($"EventSourcedRegistryGrain.Filter() :: Finished filtering grains");

            Logger.LogTrace($"EventSourcedRegistryGrain.Filter() :: Success");
            return new List<TGrain>(results);
        }

        public async Task<TResult> Fold<TResult>(Func<TResult, TGrain, TResult> folder, TResult seed)
        {
            Logger.LogTrace($"EventSourcedRegistryGrain.Fold() :: Entry");

            var state = await RegistryStateAggregator.GetValue<RegistryState>();
            Logger.LogTrace($"EventSourcedRegistryGrain.Fold() :: Obtained state");

            var results =
                from id in state.MemberGrains
                let grain = GrainFactory.GetGrain<TGrain>(id)
                select grain;
            Logger.LogTrace($"EventSourcedRegistryGrain.Fold() :: Obtained grains");

            var result = results.Aggregate(seed, folder);
            Logger.LogTrace($"EventSourcedRegistryGrain.Fold() :: Finished folding ({result})");

            Logger.LogTrace($"EventSourcedRegistryGrain.Fold() :: Success");
            return result;
        }
    }
#endif
}
