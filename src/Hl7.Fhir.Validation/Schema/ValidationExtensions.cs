using Hl7.Fhir.ElementModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema
{
    public static class ValidationExtensions
    {
        public static async Task<Assertions> Validate(this IAssertion assertion, IEnumerable<ITypedElement> input, ValidationContext vc)
        {
            switch (assertion)
            {
                case IValidatable validatable:
                    return await validatable.Validate(input, vc).ConfigureAwait(false);
                case IGroupValidatable groupvalidatable:
                    return await groupvalidatable.Validate(input, vc).ConfigureAwait(false);
                default:
                    return Assertions.Success;
            }
        }

        public static async Task<Assertions> Validate(this IAssertion assertion, ITypedElement input, ValidationContext vc)
           => await assertion.Validate(new[] { input }, vc).ConfigureAwait(false);

        public static async Task<Assertions> Validate(this IValidatable assertion, IEnumerable<ITypedElement> input, ValidationContext vc)
            => await assertion.Validate(input.SingleOrDefault(), vc).ConfigureAwait(false);
    }
}