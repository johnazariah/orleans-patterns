using System;

namespace Orleans.Patterns.EventSourcing
{
    public interface IBusinessEvent
    {
        Guid EventIdentifier { get; set; }
        DateTime EventRaised { get; set; }
        string PayloadType { get; set; }
        string PayloadJson { get; set; }
    }
}