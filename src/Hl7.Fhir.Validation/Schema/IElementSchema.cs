using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Hl7.Fhir.Validation.Schema
{
    public interface IElementSchema : IAssertion, IGroupValidatable
    {
        Uri Id { get; }

        Assertions Members { get; }
    }

    public static class IElementSchemaExtensions
    {
        public static bool IsEmpty(this IElementSchema elementSchema)
            => !elementSchema.Members.Any();

        public static IElementSchema With(this IElementSchema elementSchema, IElementDefinitionAssertionFactory factory, IEnumerable<IAssertion> additional) =>
            factory.CreateElementSchemaAssertion(elementSchema.Id, elementSchema.Members.Union(additional));

        public static IElementSchema With(this IElementSchema elementSchema, IElementDefinitionAssertionFactory factory, params IAssertion[] additional)
            => elementSchema.With(factory, additional.AsEnumerable());

        public static void LogSchema(this IElementSchema elementSchema)
        {
            var json = elementSchema.ToJson();

            Debug.WriteLine($"Elementschema: {elementSchema.Id}\n{json}");
        }
    }
}
