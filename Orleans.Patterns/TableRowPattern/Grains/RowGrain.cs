using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Orleans.Patterns.TableRowPattern
{
    public abstract class RowGrain<TState> : Grain<TState>, IRowGrain<TState>
        where TState : IStateWithIdentity, new()
    {
        protected ILogger Logger;
        public RowGrain(ILogger<RowGrain<TState>> logger) => Logger = logger;

        protected async Task EnsureState(bool write = false)
        {
            if (State == null)
            {
                State = new TState();

                if (write)
                {
                    await WriteStateAsync();
                    Logger.LogInformation("RowTableGrain.EnsureState :: Persisted State");
                }
            }
        }

        public Task<(Guid, TState)> GetIdentityAndState() => Task.FromResult((State.Id, State));

        public async Task UpdateState(TState state)
        {
            Logger.LogInformation("RowGrain.UpdateState called with [{State}]", State);

            State = state;
            await WriteStateAsync();

            Logger.LogInformation("RowGrain.UpdateState completed", State);
        }
    }
}
