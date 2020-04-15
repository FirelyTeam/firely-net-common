using System.Collections.Generic;

namespace Hl7.Fhir.Model
{
    public interface IConcept
    {
        IEnumerable<ICoding> Codes { get; }

        string Display { get; }
    }
}
