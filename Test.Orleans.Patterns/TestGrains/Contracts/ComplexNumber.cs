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

        public static ComplexNumber Zero =>
            new ComplexNumber(0.0, 0.0);

        public double RealComponent { get; set; }

        public double ImaginaryComponent { get; set; }

        /// <summary>
        /// It is no longer necessary to provide conversions from an old version DTO to an new version DTO
        /// if explicit support is provided to consume and aggregate old version DTOs in the new version folder function
        /// </summary>
        //public static implicit operator ComplexNumber(Number n) => new ComplexNumber(n.Value, 0.0);

        /// <summary>
        /// By convention, provide an aggregator to process and aggregate old version DTOs
        /// </summary>
        public ComplexNumber Combine(Number n) =>
            new ComplexNumber(RealComponent + (n?.Value ?? 0.0), ImaginaryComponent);

        /// <summary>
        /// By convention, it will be necessary to provide conversions from this new version DTO to an old version DTO to be able to support roll-back.
        /// </summary>
        /// <param name="n"></param>
        public static implicit operator Number(ComplexNumber n) =>
            new Number(n?.RealComponent ?? 0.0);
    }
}
