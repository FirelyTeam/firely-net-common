using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
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

        public JToken ToJson()
        {
            if (_members.Count() == 0) return null; // this should not happen

            if (_members.Count() == 1) return _members.First().ToJson();

            return new JProperty("any", new JArray(_members.Select(m => new JObject(m.ToJson()))));
        }


        public async Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            var validatableMembers = _members.OfType<IValidatable>();

            if (validatableMembers.Count() == 0) return Assertions.Success;

            // To not pollute the output if there's just a single input, just add it to the output
            if (validatableMembers.Count() == 1) return await validatableMembers.First().Validate(input, vc).ConfigureAwait(false);

            var result = Assertions.Empty;

            foreach (var member in validatableMembers)
            {
                var singleResult = await member.Validate(input, vc).ConfigureAwait(false);
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
            var validatableMembers = _members.OfType<IGroupValidatable>();

            if (validatableMembers.Count() == 0) return Assertions.Success;

            // To not pollute the output if there's just a single input, just add it to the output
            if (validatableMembers.Count() == 1) return await validatableMembers.First().Validate(input, vc).ConfigureAwait(false);

            var result = Assertions.Empty;

            foreach (var member in validatableMembers)
            {
                var singleResult = await member.Validate(input, vc).ConfigureAwait(false);
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
                var singleResult = await member.Validate(input, vc).ConfigureAwait(false);
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
