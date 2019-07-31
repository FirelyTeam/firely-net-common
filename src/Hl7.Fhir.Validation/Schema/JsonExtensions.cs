using Newtonsoft.Json.Linq;

namespace Hl7.Fhir.Validation.Schema
{
    public static class JsonExtensions
    {
        public static JToken MakeNestedProp(this JToken t) => t is JProperty ? new JObject(t) : t;
    }
}
