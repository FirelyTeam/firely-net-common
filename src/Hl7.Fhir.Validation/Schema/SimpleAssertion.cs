using Hl7.Fhir.ElementModel;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema
{
    public abstract class SimpleAssertion : IAssertion, IValidatable
    {
        //public IEnumerable<Assertions> Collect() => new Assertions(this).Collection;

        public SimpleAssertion(string location)
        {
            Location = location;
        }

        public virtual JToken ToJson() => new JProperty(Key, Value);

        public abstract Task<Assertions> Validate(ITypedElement input, ValidationContext vc);

        public string Location { get; }
        public abstract string Key { get; }
        public abstract object Value { get; }
    }
}
