using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Validation.Schema
{
    public class Definitions : IAssertion
    {
        public readonly ElementSchema[] Schemas;

        public Definitions(params ElementSchema[] schemas) : this(schemas.AsEnumerable()) { }

        public Definitions(IEnumerable<ElementSchema> schemas) => Schemas = schemas.ToArray();

        public JToken ToJson() =>
            new JProperty("definitions", new JArray(
                Schemas.Select(s => s.ToJson())));
    }
}
