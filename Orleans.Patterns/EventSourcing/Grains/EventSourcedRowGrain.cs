using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Orleans.Patterns.EventSourcing
{
    [Serializable]
    internal class RowState
    {
        public bool IsDeleted { get; set; } = false;

        public string RowJson { get; set; } = "{}";
    }

    internal enum RowOperation { MarkDeleted = 0, MarkUndeleted, SetRowJson }

    internal interface IEventSourcedRowAggregator : IEventAggregatorGrain { }

    internal class EventSourcedRowAggregator : EventAggregatorGrain<RowState>, IEventSourcedRowAggregator
    {
        public EventSourcedRowAggregator(IConfiguration configuration, ILogger<EventSourcedRowAggregator> logger)
            : base(configuration, logger) { }

        protected override Func<(Guid, DateTimeOffset, RowState)> InitializeSeed(RowState seed) =>
            () => (Guid.Empty, DateTimeOffset.MinValue, seed ?? new RowState());

        protected override (Guid, DateTimeOffset, RowState) ProcessEvent((Guid, DateTimeOffset, RowState) seed, BusinessEvent curr)
        {
            var (seedId, seedTimestamp, seedPayload) = seed;

            var (id, timestamp) =
                (curr.EventTimestamp > seedTimestamp)
                ? (curr.EventIdentifier, curr.EventTimestamp)
                : (seedId, seedTimestamp);

            Logger.LogTrace($"EventSourcedRegistryAggregator.ProcessEvent :: Processing Event {id}");

            var result = seedPayload ?? new RowState();
            switch (curr.BusinessEventEnum)
            {
                default:
                case (int)RowOperation.MarkDeleted:
                    {
                        var currPayload = curr.GetValue<Guid>();
                        Logger.LogTrace($"EventSourcedRegistryAggregator.ProcessEvent :: MarkDeleted {currPayload}");
                        result.IsDeleted = true;
                    }
                    break;
                case (int)RowOperation.MarkUndeleted:
                    {
                        var currPayload = curr.GetValue<Guid>();
                        Logger.LogTrace($"EventSourcedRegistryAggregator.ProcessEvent :: MarkUndeleted {currPayload}");
                        result.IsDeleted = false;
                    }
                    break;
                case (int)RowOperation.SetRowJson:
                    {
                        var currPayload = curr.GetValue<string>();
                        Logger.LogTrace($"EventSourcedRegistryAggregator.ProcessEvent :: SetJsonDocument {currPayload}");
                        result.RowJson = currPayload;
                    }
                    break;
            }
            return (id, timestamp, result);
        }
    }

    public class EventSourcedRowGrain : EventSourcedGrain, IEventSourcedRowGrain
    {
        public EventSourcedRowGrain(IConfiguration configuration, ILogger<EventSourcedRowGrain> logger)
            : base(configuration, logger)
        { }

        public override async Task OnActivateAsync()
        {
            Logger.LogTrace($"EventSourcedRowGrain.OnActivateAsync :: Entry");

            var id = this.GetPrimaryKey();
            Logger.LogTrace($"EventSourcedRowGrain.OnActivateAsync :: Primary Key : {id}");

            RowStateAggregator = await RegisterAggregateGrain<IEventSourcedRowAggregator>();
            Logger.LogTrace($"EventSourcedRowGrain.OnActivateAsync :: Obtained and set RegistryStateAggregator synchronously");

            Logger.LogTrace($"EventSourcedRowGrain.OnActivateAsync :: Success");
        }

        internal IEventSourcedRowAggregator RowStateAggregator { get; private set; }

        public Task<Guid> GetIdentity()
        {
            Logger.LogTrace($"EventSourcedRowGrain.GetIdentity() :: Entry");

            var id = this.GetPrimaryKey();
            Logger.LogTrace($"EventSourcedRowGrain.GetIdentity() :: Returning [{id}]");

            Logger.LogTrace($"EventSourcedRowGrain.GetIdentity() :: Success");
            return Task.FromResult(id);
        }

        public async Task<string> GetRowJson()
        {
            Logger.LogTrace($"EventSourcedRowGrain.GetJsonDocument() :: Entry");

            var state = await RowStateAggregator.GetValue<RowState>();
            Logger.LogTrace($"EventSourcedRowGrain.GetJsonDocument() :: Obtained state");

            Logger.LogTrace($"EventSourcedRowGrain.IsDeleted() :: Returning [{state.RowJson}]");

            Logger.LogTrace($"EventSourcedRowGrain.GetJsonDocument() :: Success");
            return state.RowJson;
        }

        public async Task<bool> IsDeleted()
        {
            Logger.LogTrace($"EventSourcedRowGrain.IsDeleted() :: Entry");

            var state = await RowStateAggregator.GetValue<RowState>();
            Logger.LogTrace($"EventSourcedRowGrain.IsDeleted() :: Obtained state");

            Logger.LogTrace($"EventSourcedRowGrain.IsDeleted() :: Returning [{state.IsDeleted}]");

            Logger.LogTrace($"EventSourcedRowGrain.IsDeleted() :: Success");
            return state.IsDeleted;
        }

        public async Task MarkDeleted()
        {
            var id = this.GetPrimaryKey();
            Logger.LogTrace($"EventSourcedRowGrain.MarkDeleted({id}) :: Entry");
            await RecordEventPayload((int)RowOperation.MarkDeleted, id);

            Logger.LogTrace($"EventSourcedRowGrain.MarkDeleted({id}) :: Success");
        }

        public async Task MarkUndeleted()
        {
            var id = this.GetPrimaryKey();
            Logger.LogTrace($"EventSourcedRowGrain.MarkUndeleted({id}) :: Entry");
            await RecordEventPayload((int)RowOperation.MarkUndeleted, id);

            Logger.LogTrace($"EventSourcedRowGrain.MarkUndeleted({id}) :: Success");
        }

        public async Task SetRowJson(string payload)
        {
            var id = this.GetPrimaryKey();
            Logger.LogTrace($"EventSourcedRowGrain.SetJsonDocument({id}, {payload}) :: Entry");
            await RecordEventPayload((int)RowOperation.SetRowJson, payload);

            Logger.LogTrace($"EventSourcedRowGrain.SetJsonDocument({id}) :: Success");
        }
    }
}
