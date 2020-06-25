using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class AllAssertion : IValidatable
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
            if (_members.Count() == 0) return null; // this should not happen

            if (_members.Count() == 1) return _members.First().ToJson();

            return new JProperty("all", new JArray(_members.Select(m => new JObject(m.ToJson()))));
        }

        public async Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            var result = Assertions.Empty;

            foreach (var member in _members.OfType<IValidatable>())
            {
                var memberResult = await member.Validate(input, vc).ConfigureAwait(false);
                if (!memberResult.Result.IsSuccessful)
                {
                    // we have found a failure result, so we do not continue with the rest anymore
                    return memberResult;
                }
                result += memberResult;
            }
            return result;
        }
    }
}
