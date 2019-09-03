using System;

namespace Hl7.Fhir.Language
{
    public class OptionTypeSpecifier : TypeSpecifier
    {
        public OptionTypeSpecifier(TypeSpecifier option)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
        }

        public TypeSpecifier Option { get; private set; }

        public override string ToString() => Option.ToString() + "?";
    }

}
