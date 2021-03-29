using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hl7.Fhir.Model
{
    public static class ParametersExtensions
    {
        public static bool TryGetDuplicates(this Parameters parameters , out List<string> duplicates)
        {
            duplicates = new List<string>() { };
            return parameters.Parameter?.Select(p => p.Name)?.TryGetDuplicates(out duplicates) == true;            
        }
    }
}
