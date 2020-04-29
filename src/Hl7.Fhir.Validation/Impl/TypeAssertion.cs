using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class TypeAssertion : IAssertion, IValidatable
    {
        public JToken ToJson()
        {
            throw new System.NotImplementedException();
        }

        public Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            throw new System.NotImplementedException();
        }
    }
}
