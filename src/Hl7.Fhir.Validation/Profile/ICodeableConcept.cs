using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public interface ICodeableConcept
    {
        ICollectionFacade<ICodingExt> Coding { get; }
        string Text { get; set; }
    }
}
