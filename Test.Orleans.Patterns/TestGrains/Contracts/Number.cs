using System;

namespace Test.Orleans.Patterns.Contracts
{
    [Serializable]
    public class Number
    {
        public Number(double value) => Value = value;

        public double Value { get; set; }
    }
}
