using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using Hl7.FhirPath;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class PathSelectorAssertion : IAssertion, IValidatable
    {
        private readonly string _path;
        private readonly IAssertion _other;

        public PathSelectorAssertion(string path, IAssertion other)
        {
            _path = path;
            _other = other;
        }

        public async Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            return await _other.Validate(input.Select(_path), vc);
        }

        public JToken ToJson()
        {
            var props = new JObject()
            {
                new JProperty("path", _path),
                new JProperty("assertion", new JObject(_other.ToJson()))

            };

            return new JProperty("pathSelector", props);
        }



    }
}
