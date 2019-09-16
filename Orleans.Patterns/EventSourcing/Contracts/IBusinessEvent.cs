using System;

namespace Orleans.Patterns.EventSourcing
{
    public interface IBusinessEvent
    {
        int BusinessEventEnum         { get; set; }
        Guid EventIdentifier          { get; set; }
        DateTimeOffset EventTimestamp { get; set; }
        string PayloadType            { get; set; }
        string PayloadJson            { get; set; }
    }
}