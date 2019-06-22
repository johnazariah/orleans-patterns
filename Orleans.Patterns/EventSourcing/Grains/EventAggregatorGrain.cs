using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        public DateTime? LastEventRaised { get; set; } = null;

        public List<(BusinessEvent, Exception)> Failures { get; set; } = new List<(BusinessEvent, Exception)>();

        public void SetValue<T>(T payload)
        {
            PayloadType = payload.GetType().AssemblyQualifiedName;
            PayloadJson = JsonConvert.SerializeObject(payload);
        }

        public T GetValue<T>()
        {
            try
            {
                var payloadType = Type.GetType(PayloadType);
                return (T)(JsonConvert.DeserializeObject(PayloadJson, payloadType));
            }
            catch
            {
                return default(T);
            }
        }
    }

    public abstract class EventAggregatorGrain<T>: Grain<EventAggregatorState>, IEventAggregatorGrain
    {
        private readonly ILogger Logger;

        protected EventAggregatorGrain(CloudTable eventsTable, ILogger<EventAggregatorGrain<T>> logger)
        {
            EventsTable = eventsTable;
            Logger = logger;
        }

        protected CloudTable EventsTable { get; }

        public virtual async Task RefreshState()
        {
            var ((latestEvent, latestEventRaised, value), failures) =
                await EventsTable.FoldEventsAsync(
                    this.GetPrimaryKey(),
                    ProcessEvent,
                    (Guid.Empty, DateTime.MaxValue, State.GetValue<T>()),
                    State.LastEventRaised);

            State.LastEvent = latestEvent;
            State.LastEventRaised = latestEventRaised;
            State.Failures = failures;
            State.SetValue(value);

            await WriteStateAsync();
        }

        protected abstract (Guid, DateTime, T) ProcessEvent((Guid, DateTime, T) seed, BusinessEvent curr);

        public virtual async Task<T> GetValue()
        {
            await RefreshState();
            return State.GetValue<T>();
        }

        async Task<X> IEventAggregatorGrain.GetValue<X>() =>
            (await GetValue()).DynamicCast<X>();
    }
}
