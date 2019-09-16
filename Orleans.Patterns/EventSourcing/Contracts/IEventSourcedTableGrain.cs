using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Patterns.EventSourcing
{
    public interface IEventSourcedTableGrain<TRowGrain> : IEventSourcedRegistryGrain<TRowGrain>
        where TRowGrain : IEventSourcedRowGrain
    {
        Task<TRowGrain> CreateRow(Guid id);

        Task DeleteRow(TRowGrain candidate);

        Task<HashSet<Guid>> GetRows();
    }
}
