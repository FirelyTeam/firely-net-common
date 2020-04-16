/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model.Primitives;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation.Schema;

namespace Hl7.Fhir.Validation.Impl
{
    public class MaxLength : SimpleAssertion
    {
        private readonly int _maxLength;

        public MaxLength(string location, int maxLength) : base(location)
        {
            if (maxLength <= 0)
                throw new IncorrectElementDefinitionException($"{nameof(maxLength)}: Must be a positive number");

            _maxLength = maxLength;
        }

        public override string Key => "maxLength";

        public override object Value => _maxLength;

        public override Assertions Validate(ITypedElement input, ValidationContext vc)
        {
            if (input == null) throw Error.ArgumentNull(nameof(input));

            var result = Assertions.Empty + this;

            if (Any.ConvertToSystemValue(input.Value) is string serializedValue)
            {
                result += new Trace($"Maxlength validation with value '{serializedValue}'");

                if (serializedValue.Length > _maxLength)
                {
                    return result + Assertions.Failure + new IssueAssertion(1005, input.Location, "message") + new Trace($"Value '{serializedValue}' is too long (maximum length is {_maxLength})");
                }
            }
            else return Assertions.Undecided;

            return result + Assertions.Success;
        }

    }
}