using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Patterns.TableRowPattern
{
    public static class Configuration
    {
        public static readonly Guid SingletonTableGrainId = Guid.Parse("3471042a-a614-11e8-98d0-529269fb1459");
    }

    public interface IRowTableGrain<TGrain, TState> : IGrainWithGuidKey
        where TGrain : IRowGrain<TState>
        where TState : IStateWithIdentity, new()
    {
        Task<TGrain> CreateRow(Guid id, TState state);
        Task DeleteRow(Guid id);
        Task<List<TState>> GetRows();
    }
}
