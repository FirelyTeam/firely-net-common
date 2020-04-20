using Hl7.Fhir.ElementModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema
{
    public interface IElementSchema: IAssertion, IGroupValidatable
    {
        Uri Id { get; }

        Assertions Members { get; }
    }

    public static class IElementSchemaExtensions
    {
        public static bool IsEmpty(this IElementSchema elementSchema) 
            => !elementSchema.Members.Any();

        public static IElementSchema With(this IElementSchema elementSchema, IAssertionFactory factory, IEnumerable<IAssertion> additional) =>
            factory.CreateElementSchemaAssertion(elementSchema.Id, elementSchema.Members.Union(additional));

        public static IElementSchema With(this IElementSchema elementSchema, IAssertionFactory factory, params IAssertion[] additional) 
            => elementSchema.With(factory, additional);
    }
}
