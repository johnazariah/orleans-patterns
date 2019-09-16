using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Patterns.EventSourcing
{
    public interface IEventSourcedRegistryGrain<TGrain> : IEventSourcedGrain
        where TGrain : IEventSourcedGrain
    {
        Task RegisterGrain(TGrain candidateGrain);
        Task UnregisterGrain(TGrain candidateGrain);

        Task<HashSet<Guid>> GetMemberGrains();
    }
}
