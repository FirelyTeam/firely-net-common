using Castle.Core.Internal;
using Hl7.Fhir.ElementModel;

namespace Hl7.Fhir.Validation.Tests.Support
{
    public static class Foo
    {
        public static ITypedElement CreateHumanName(string familyName, string[] givenNames)
        {
            var node = ElementNode.Root("HumanName");
            if (familyName.IsNullOrEmpty())
                node.Add("family", familyName, "string");
            foreach (var givenName in givenNames)
                node.Add("given", givenName, "string");
            return node;
        }
    }
}
