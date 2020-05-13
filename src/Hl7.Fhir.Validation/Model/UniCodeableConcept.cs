using System.Collections.Generic;

namespace Hl7.Fhir.Validation.Model
{
    public class UniCodeableConcept: IUniDataType
    {
        public List<UniCoding> Codings { get; set; }
        public string Text { get; set; }
    }
}
