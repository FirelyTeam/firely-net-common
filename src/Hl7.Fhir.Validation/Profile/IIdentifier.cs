using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public enum IdentifierUse
    {
        Usual,
        Official,
        Temporary,
        Secondary,
        Old
    }

    public interface IIdentifier
    {
        IdentifierUse? Use { get; set; }
        ICodeableConcept Type { get; }
        string System { get; set; }
        string Value { get; set; }
        IPeriod Period { get; }
        IReference Assigner { get; }
    }
}
