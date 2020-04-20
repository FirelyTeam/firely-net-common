using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class AnyAssertion : IAssertion, IValidatable
    {
        private readonly IAssertion[] _members;

        public AnyAssertion(IEnumerable<IAssertion> assertions)
        {
            _members = assertions.ToArray();
        }

        public JToken ToJson()
        {
            throw new NotImplementedException();
        }

        public async Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            var result = Assertions.Empty;

            foreach (var member in _members.OfType<IValidatable>())
            {
                var singleResult = await member.Validate(input, vc);
                if (singleResult == Assertions.Success)
                {
                    // we have found a result, so we do not continue with the rest anymore
                    return singleResult;
                }
                result += singleResult;
            }
            return result;
        }
    }
}
