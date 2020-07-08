using Hl7.Fhir.ElementModel;
using System;
using System.Collections.Generic;

namespace Hl7.FhirPath.FhirPath.Functions
{
    internal static class MathOperators
    {
        public static IEnumerable<ITypedElement> Sqrt(this decimal focus)
        {
            var result = Math.Sqrt(Convert.ToDouble(focus));
            if (result != Double.NaN) yield return ElementNode.ForPrimitive(result);
        }
    }
}
