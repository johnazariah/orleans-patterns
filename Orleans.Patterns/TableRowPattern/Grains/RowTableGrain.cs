using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Patterns.TableRowPattern
{
    public class RowTableGrain<TGrain, TState> : Grain<HashSet<Guid>>, IRowTableGrain<TGrain, TState>
        where TGrain : IRowGrain<TState>
        where TState : IStateWithIdentity, new()
    {
        private readonly ILogger Logger;
        public RowTableGrain(ILogger<RowTableGrain<TGrain, TState>> logger) => Logger = logger;

        protected async Task EnsureState(bool write = false)
        {
            if (State == null)
            {
                State = new HashSet<Guid>();

                if (write)
                {
                    await WriteStateAsync();
                    Logger.LogInformation("RowTableGrain.EnsureState :: Persisted State");
                }
            }
        }

        public async Task<List<TState>> GetRows()
        {
            Console.WriteLine ("RowTableGrain.GetRows :: Entry");

            await EnsureState();
            Console.WriteLine ("RowTableGrain.GetRows :: State Ensured");

            Logger.LogInformation("RowTableGrain.GetRows returned [{Rows}]", State.Count);

            var tasks =
                from id in State
                let grain = GrainFactory.GetGrain<TGrain>(id)
                select grain.GetIdentityAndState();

            Console.WriteLine ("RowTableGrain.GetRows :: Tasks Computed");

            var result =
                from v in await Task.WhenAll(tasks)
                select v.Item2;

            Console.WriteLine ("RowTableGrain.GetRows :: Results Computed");

            return result.ToList();
        }

        public async Task<TGrain> CreateRow(Guid id, TState state)
        {
            Logger.LogInformation("RowTableGrain.CreateRow :: [{RowId}, {State}]", id, state);
            state.Id = id;

            var rowGrain = GrainFactory.GetGrain<TGrain>(id);
            Logger.LogInformation("RowTableGrain.CreateRow :: Obtained Grain For {RowId}]", id);

            await rowGrain.UpdateState(state);
            Logger.LogInformation("RowTableGrain.CreateRow :: Updated State For {RowId}] to {State}", id, state);

            await RegisterGrain(id, state);
            Logger.LogInformation("RowTableGrain.CreateRow :: Registered Grain for {RowId}", id);

            return rowGrain;
        }

        public async Task DeleteRow(Guid id)
        {
            Logger.LogInformation("RowTableGrain.DeleteRow :: [{RowId}]", id);

            var rowGrain = GrainFactory.GetGrain<TGrain>(id);
            Logger.LogInformation("RowTableGrain.CreateRow :: Obtained Grain For {RowId}]", id);

            await rowGrain.UpdateState(default(TState));
            Logger.LogInformation("RowTableGrain.CreateRow :: Cleared State For {RowId}]", id);

            await UnregisterGrain(id);
            Logger.LogInformation("RowTableGrain.CreateRow :: Unregistered Grain for {RowId}", id);
        }

        public async Task RegisterGrain(Guid id, TState state)
        {
            Logger.LogInformation("RowTableGrain.RegisterGrain :: [{RowId}]", id);
            await EnsureState();

            State.Add(id);
            Logger.LogInformation("RowTableGrain.RegisterGrain :: Added [{RowId}] to [{State}]", id, State);

            await WriteStateAsync();
            Logger.LogInformation("RowTableGrain.RegisterGrain :: Persisted State");
        }

        public async Task UnregisterGrain(Guid id)
        {
            Logger.LogInformation("RowTableGrain.UnregisterGrain :: [{RowId}]", id);
            await EnsureState();

            State.Remove(id);
            Logger.LogInformation("RowTableGrain.UnregisterGrain :: Removed [{RowId}] from [{State}]", id, State);

            await WriteStateAsync();
            Logger.LogInformation("RowTableGrain.UnregisterGrain :: Persisted State");
        }
    }
}
