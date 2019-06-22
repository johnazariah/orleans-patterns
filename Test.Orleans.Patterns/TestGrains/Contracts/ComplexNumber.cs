using System;

namespace Test.Orleans.Patterns.Contracts
{

    [Serializable]
    public class ComplexNumber
    {
        public ComplexNumber(double realComponent, double imaginaryComponent)
        {
            RealComponent = realComponent;
            ImaginaryComponent = imaginaryComponent;
        }

        public double RealComponent { get; set; }
        public double ImaginaryComponent { get; set; }

        public static implicit operator ComplexNumber(Number n) => new ComplexNumber(n.Value, 0.0);
        public static implicit operator Number(ComplexNumber n) => new Number(n.RealComponent);
    }
}
