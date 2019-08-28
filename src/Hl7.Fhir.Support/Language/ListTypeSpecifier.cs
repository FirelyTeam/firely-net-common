using System;

namespace Hl7.Fhir.Language
{
    public class ListTypeSpecifier : TypeSpecifier, IEquatable<ListTypeSpecifier>
    {
        public ListTypeSpecifier(TypeSpecifier elementType)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public TypeSpecifier ElementType { get; private set; }

        public bool Equals(ListTypeSpecifier other) => throw new NotImplementedException();

        public override string ToString() => $"List<{ElementType.ToString()}>";
    }
}
