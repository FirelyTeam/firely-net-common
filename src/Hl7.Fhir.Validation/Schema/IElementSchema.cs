using Hl7.Fhir.ElementModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema
{
    public interface IElementSchema: IAssertion
    {
        Uri Id { get; }

        Assertions Members { get; }

        IElementSchema With(IEnumerable<IAssertion> additional);

        Assertions Validate(IEnumerable<ITypedElement> input, ValidationContext vc);
    }

    public static class IElementSchemaExtensions
    {
        public static bool IsEmpty(this IElementSchema elementSchema) 
            => !elementSchema.Members.Any();

        public static IElementSchema With(this IElementSchema elementSchema, params IAssertion[] additional) 
            => elementSchema.With(additional);
    }
}
