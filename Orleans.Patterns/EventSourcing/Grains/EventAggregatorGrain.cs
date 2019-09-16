using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans.Patterns.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Patterns.EventSourcing
{
    [Serializable]
    public class EventAggregatorState
    {
        public string PayloadType { get; set; }
        public string PayloadJson { get; set; }
        public Guid LastEvent { get; set; }
        public DateTimeOffset? LastEventTimestamp { get; set; } = null;

        public List<(BusinessEvent, Exception)> Failures { get; set; } = new List<(BusinessEvent, Exception)>();

        public void SetValue<T>(T payload)
        {
            PayloadType = typeof(T).AssemblyQualifiedName;
            PayloadJson = JsonConvert.SerializeObject(payload);
        }

        public T GetValue<T>()
        {
            try
            {
                var payloadType = Type.GetType(PayloadType);
                return (T)JsonConvert.DeserializeObject(PayloadJson, payloadType);
            }
            catch
            {
                return default;
            }
        }
    }

    public abstract class EventAggregatorGrain<T>: Grain<EventAggregatorState>, IEventAggregatorGrain
    {
        protected EventAggregatorGrain(IConfiguration configuration, ILogger<EventAggregatorGrain<T>> logger) :
            this(configuration.EventsTable(), logger)
        { }

        protected EventAggregatorGrain(CloudTable eventsTable, ILogger<EventAggregatorGrain<T>> logger)
        {
            EventsTable = eventsTable;
            Logger = logger;
        }

        protected CloudTable EventsTable { get; }
        protected ILogger Logger { get; }

        public virtual async Task RefreshState()
        {
            var ((latestEvent, latestEventRaised, value), failures) =
                await EventsTable.FoldEventsAsync(
                    this.GetPrimaryKey(),
                    ProcessEvent,
                    InitializeSeed(State.GetValue<T>()),
                    State.LastEventTimestamp);

            State.LastEvent = latestEvent;
            State.LastEventTimestamp = latestEventRaised;
            State.Failures = failures;
            State.SetValue(value);

            await WriteStateAsync();
        }

        protected abstract (Guid, DateTimeOffset, T) ProcessEvent((Guid, DateTimeOffset, T) seed, BusinessEvent curr);
        protected abstract Func<(Guid, DateTimeOffset, T)> InitializeSeed(T seed);

        public virtual async Task<T> GetValue()
        {
            await RefreshState();
            return State.GetValue<T>();
        }

        async Task<X> IEventAggregatorGrain.GetValue<X>() =>
            (await GetValue()).DynamicCast<X>();
    }
}
