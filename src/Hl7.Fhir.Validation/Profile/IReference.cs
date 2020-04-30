using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public interface IReference
    {
        string Reference { get; set; }
        string Type { get; set; }
        IIdentifier Identifier { get; }
        string Display { get; set; }
    }
}
