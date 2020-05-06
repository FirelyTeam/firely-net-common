using System.Collections.Generic;

namespace Hl7.Fhir.Validation.Model
{
    public class UniContactDetail
    {
        public string Name { get; set; }
        public List<UniContactPoint> Telecoms { get; set; }
    }
}
