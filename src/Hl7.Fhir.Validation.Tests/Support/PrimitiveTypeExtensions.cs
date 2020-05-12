using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;

namespace Hl7.Fhir.Validation.Tests.Support
{
    /// <summary>
    /// This class is just temporarily and should be replaced when the new Typesystem has been implemented
    /// https://gist.github.com/ewoutkramer/30fbb9b62c4f493dc479129f80ad0e23
    /// </summary>
    internal static class PrimitiveTypeExtensions
    {
        public static ITypedElement ToTypedElement(this PrimitiveType primitiveType)
            => ElementNode.Root(primitiveType.TypeName, name: primitiveType.GetType().Name, value: primitiveType.ObjectValue);

        public static ITypedElement ToTypedElement<T, V>(V value) where T : PrimitiveType, IValue<V>, new()
        {
            var instance = new T
            {
                ObjectValue = value
            };
            return instance.ToTypedElement();
        }
    }
}
