using System.Collections.Generic;

namespace Hl7.Fhir.Validation.Model
{
    public class UniCodeableConcept: IUniElement
    {
        public List<UniCoding> Codings { get; set; }
        public string Text { get; set; }
    }
}
