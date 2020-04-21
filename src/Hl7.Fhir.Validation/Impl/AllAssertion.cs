using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class AllAssertion : IAssertion, IValidatable
    {
        private readonly IAssertion[] _members;

        public AllAssertion(IEnumerable<IAssertion> assertions)
        {
            _members = assertions.ToArray();
        }

        public AllAssertion(params IAssertion[] assertions) : this(new Assertions(assertions))
        {
        }

        public JToken ToJson()
        {
            var result = new JObject
            {
                _members.Select(mem => nest(mem.ToJson()))
            };
            return result;

            JToken nest(JToken mem) =>
                mem is JObject ? new JProperty("nested", mem) : mem;
        }

        public async Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            var result = Assertions.Empty;

            foreach (var member in _members.OfType<IValidatable>())
            {
                var memberResult = await member.Validate(input, vc);
                if (memberResult.Result.IsSuccessful)
                {
                    // we have found a result, so we do not continue with the rest anymore
                    return memberResult;
                }
                result += memberResult;
            }
            return result;
        }
    }
}
