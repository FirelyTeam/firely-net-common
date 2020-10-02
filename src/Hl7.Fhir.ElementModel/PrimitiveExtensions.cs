using Hl7.Fhir.ElementModel.Types;
using Hl7.Fhir.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.ElementModel
{
    public static class PrimitiveExtensions
    {
        public static object ParseBindable(this ITypedElement instance)
        {
            switch (instance.InstanceType)
            {
                case "code":
                case "string":
                case "uri":
                    return instance.ParseString();
                case "Coding":
                    return instance.ParseCoding();
                case "CodeableConcept":
                    return instance.ParseConcept();
                case "Quantity":
                    return convertQuantityToCoding(instance);
                case "Extension":
                    return parseExtension(instance);
                default:
                    return null;
            }

            Code convertQuantityToCoding(ITypedElement inst)
            {
                var q = inst.ParseQuantity();
                return new Code(code: q.Unit, system: q.System == QuantityUnitSystem.UCUM ? Quantity.UCUM : null);
            }

            object parseExtension(ITypedElement inst)
            {
                var valueChild = inst.Children("value").FirstOrDefault();
                return valueChild?.ParseBindable();
            }
        }

        public static Code ParseCoding(this ITypedElement instance) =>
            new Code(
                instance.Children("system").GetString(),
                instance.Children("code").GetString(),
                instance.Children("display").GetString());

        public static Concept ParseConcept(this ITypedElement instance) =>
            new Concept(
                instance.Children("coding").Select(coding => coding.ParseCoding()).ToArray(),
                instance.Children("text").GetString());

        public static string GetString(this IEnumerable<ITypedElement> instance) => instance.SingleOrDefault()?.Value as string;

        public static string ParseString(this ITypedElement instance) => instance?.Value as string;

        public static Quantity ParseQuantity(this ITypedElement instance)
        {
            var value = instance.Children("value").SingleOrDefault()?.Value as decimal?;
            var code = instance.Children("code").GetString();


            if (value == null)
                throw Error.NotSupported("Cannot interpret quantities without a value");

            return new Quantity(value.Value, code);
        }
    }
}
