using System;

namespace Hl7.FhirPath.FhirPath.Functions
{
    internal static class MathOperators
    {
        public static decimal? Sqrt(this decimal focus)
        {
            var result = Math.Sqrt(Convert.ToDouble(focus));
            return double.IsNaN(result) ? (decimal?)null : Convert.ToDecimal(result);
        }

        public static decimal? Power(this decimal focus, decimal exponent)
        {
            var result = Math.Pow(Convert.ToDouble(focus), Convert.ToDouble(exponent));
            return double.IsNaN(result) ? (decimal?)null : Convert.ToDecimal(result);
        }
    }
}
