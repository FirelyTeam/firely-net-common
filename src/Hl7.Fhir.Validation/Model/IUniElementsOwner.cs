using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Model
{
    public interface IUniElementsOwner
    {
        List<UniStructureDefElement> Elements { get; set; }
    }
}
