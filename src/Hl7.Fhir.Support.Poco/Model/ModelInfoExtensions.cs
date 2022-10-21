using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using System;

#nullable enable

namespace Hl7.Fhir.Support.Poco.Model
{
    public class ModelInfoExtensions
    {
        public static readonly Uri FhirCoreProfileBaseUri = new(@"http://hl7.org/fhir/StructureDefinition/");

        public static Canonical CanonicalUriForFhirCoreType(string typename) => new(FhirCoreProfileBaseUri + typename);

        public static string? GetFhirTypeNameForType(Type type) => ModelInspector.ForAssembly(type.Assembly).GetFhirTypeNameForType(type);
        public static bool IsPrimitive(string name) => ModelInspector.ForAssembly(typeof(PrimitiveType).Assembly).IsPrimitive(name);
        public static bool IsPrimitive(Type type) => ModelInspector.ForAssembly(typeof(PrimitiveType).Assembly).IsPrimitive(type);
        public static bool IsBindable(string type) => ModelInspector.ForAssembly(typeof(PrimitiveType).Assembly).IsBindable(type);
    }
}
#nullable restore