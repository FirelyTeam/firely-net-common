using System;
using System.Linq;

namespace Hl7.Fhir.Language
{

    public class ChoiceTypeSpecifier : TypeSpecifier, IEquatable<ChoiceTypeSpecifier>
    {
        public ChoiceTypeSpecifier(TypeSpecifier[] choices)
        {
            Choices = choices ?? throw new ArgumentNullException(nameof(choices));
        }

        public TypeSpecifier[] Choices { get; private set; }

        public bool Equals(ChoiceTypeSpecifier other) => throw new NotImplementedException();

        public override string ToString()
        {
            var choices = Choices.Select(c => c.ToString());
            return $"Choice<{string.Join(", ", choices)}>";
        }
            
    }
}
