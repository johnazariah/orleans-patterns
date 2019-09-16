using System;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace Orleans.Patterns.EventSourcing
{
    [Serializable]
    public class BusinessEvent : TableEntity, IBusinessEvent
    {
        public BusinessEvent() {}

        public BusinessEvent(int businessEventEnum, string payloadType, string payloadJson)
        {
            BusinessEventEnum = businessEventEnum;
            EventIdentifier = Guid.NewGuid();
            EventTimestamp = DateTime.UtcNow;
            PayloadType = payloadType;
            PayloadJson = payloadJson;

            RowKey = EventTimestamp.Ticks.ToString();
        }

        public int BusinessEventEnum         { get; set; }
        public Guid EventIdentifier          { get; set; }
        public DateTimeOffset EventTimestamp { get; set; }
        public string PayloadType            { get; set; }
        public string PayloadJson            { get; set; }

        public T GetValue<T>()
        {
            var payloadType = Type.GetType(PayloadType);
            var storedVersionPayload = JsonConvert.DeserializeObject(PayloadJson, payloadType);
            var returnVersionPayload = storedVersionPayload.DynamicCast<T>();
            return returnVersionPayload;
        }
    }

    [Serializable]
    public sealed class BusinessEvent<T> : BusinessEvent
    {
        public BusinessEvent(int businessEventEnum, T payload)
            : base(
                  businessEventEnum,
                  payload.GetType().AssemblyQualifiedName,
                  JsonConvert.SerializeObject(payload))
        {
            Payload = payload;
        }

        public T Payload { get; }

        public static BusinessEvent<T> Read(BusinessEvent raw)
        {
            var payload = JsonConvert.DeserializeObject<T>(raw.PayloadJson);
            return new BusinessEvent<T>(raw.BusinessEventEnum, payload);
        }
    }
}