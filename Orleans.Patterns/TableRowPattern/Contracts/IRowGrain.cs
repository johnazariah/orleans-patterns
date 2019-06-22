using System;
using System.Threading.Tasks;

namespace Orleans.Patterns.TableRowPattern
{
    public interface IRowGrain<TState> : IGrainWithGuidKey where TState : IStateWithIdentity, new()
    {
        Task<(Guid, TState)> GetIdentityAndState();
        Task UpdateState(TState state);
    }
}
