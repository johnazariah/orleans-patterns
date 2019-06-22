using System;
using Orleans.Patterns.TableRowPattern;

namespace Test.Orleans.Patterns.Contracts
{
    public class TestRowState : IStateWithIdentity
    {
        public Guid Id { get; set; }
        public int Value { get; set; }
    }
}
