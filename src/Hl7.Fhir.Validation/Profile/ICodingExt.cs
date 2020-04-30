using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public interface ICodingExt
    {
        string System { get; set; }
        string Version { get; set; }
        string Code { get; set; }
        string Display { get; set; }
        bool? UserSelected { get; set; }
    }
}
