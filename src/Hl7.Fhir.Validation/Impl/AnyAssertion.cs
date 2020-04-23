using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class AnyAssertion : IAssertion, IValidatable, IGroupValidatable
    {
        private readonly IAssertion[] _members;

        public AnyAssertion(IEnumerable<IAssertion> assertions)
        {
            _members = assertions.ToArray();
        }

        public JToken ToJson() =>
            new JProperty("any", new JObject() { _members.Select(m =>
                new JProperty(Guid.NewGuid().ToString(), m.ToJson().MakeNestedProp())) });

        public async Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            var result = Assertions.Empty;

            foreach (var member in _members.OfType<IValidatable>())
            {
                var singleResult = await member.Validate(input, vc);
                result += singleResult;
                if (singleResult.Result.IsSuccessful)
                {
                    // we have found a result, so we do not continue with the rest anymore
                    return singleResult;
                }
            }
            return Assertions.Failure + new IssueAssertion(1, "TODO", "any did not succeed", IssueSeverity.Error) + result;
        }

        public async Task<Assertions> Validate(IEnumerable<ITypedElement> input, ValidationContext vc)
        {
            var result = Assertions.Empty;

            foreach (var member in _members.OfType<IGroupValidatable>())
            {
                var singleResult = await member.Validate(input, vc);
                result += singleResult;
                if (singleResult.Result.IsSuccessful)
                {
                    // we have found a result, so we do not continue with the rest anymore
                    return singleResult;
                }
            }
            return Assertions.Failure + new IssueAssertion(1, "TODO", "any did not succeed", IssueSeverity.Error) + result;
        }

        private async Task<Assertions> Foo<T>(IEnumerable<ITypedElement> input, ValidationContext vc) where T : IValidatable, IGroupValidatable
        {
            var result = Assertions.Empty;

            foreach (var member in _members.OfType<T>())
            {
                var singleResult = await member.Validate(input, vc);
                result += singleResult;
                if (singleResult.Result.IsSuccessful)
                {
                    // we have found a result, so we do not continue with the rest anymore
                    return singleResult;
                }
            }
            return Assertions.Failure + new IssueAssertion(1, "TODO", "any did not succeed", IssueSeverity.Error) + result;
        }
    }
}
