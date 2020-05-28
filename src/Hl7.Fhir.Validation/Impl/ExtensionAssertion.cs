using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class ExtensionAssertion : IAssertion, IGroupValidatable
    {
        private readonly Func<Uri, Task<IElementSchema>> _getSchema;
        private readonly Uri _referencedUri;

        public ExtensionAssertion(Func<Uri, Task<IElementSchema>> getSchema, Uri reference = null)
        {
            _getSchema = getSchema;
            _referencedUri = reference;
        }

        public Uri ReferencedUri => _referencedUri;

        public async Task<Assertions> Validate(IEnumerable<ITypedElement> input, ValidationContext vc)
        {
            var groups = input.GroupBy(elt => elt.Children("url").GetString());

            var result = Assertions.Empty;

            foreach (var item in groups)
            {
                var schema = await _getSchema(new Uri(item.Key ?? "http://hl7.org/fhir/StructureDefinition/Extension")).ConfigureAwait(false);
                result += await schema.Validate(item, vc).ConfigureAwait(false);
            }

            return result.AddResultAssertion();
        }

        public JToken ToJson() => new JProperty("$extension", ReferencedUri?.ToString() ??
            throw Error.InvalidOperation("Cannot convert to Json: reference refers to a schema without an identifier"));
    }
}
