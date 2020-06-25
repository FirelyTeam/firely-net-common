using Newtonsoft.Json.Linq;
using System;

namespace Hl7.Fhir.Validation.Schema
{
    public class CompileAssertion : IAssertion
    {
        public readonly string Message;

        public JToken ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
