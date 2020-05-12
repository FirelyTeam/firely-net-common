using System.Collections.Generic;

namespace Hl7.Fhir.Validation.Model
{
    public class UniStructureDefElementSchema: IUniElementsOwner
    {
        public List<UniStructureDefElement> Elements { get; set; }
    }
}
