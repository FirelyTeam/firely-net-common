﻿using Hl7.Fhir.ElementModel;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema
{
    public abstract class SimpleAssertion : IAssertion, IValidatable
    {

        public virtual JToken ToJson() => new JProperty(Key, Value);

        public abstract Task<Assertions> Validate(ITypedElement input, ValidationContext vc);

        public abstract string Key { get; }
        public abstract object Value { get; }
    }
}
