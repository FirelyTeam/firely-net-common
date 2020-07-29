/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.ElementModel.Types;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation.Schema;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Impl
{
    public class MaxLength : SimpleAssertion
    {
        private readonly int _maxLength;

        public MaxLength(int maxLength)
        {
            if (maxLength <= 0)
                throw new IncorrectElementDefinitionException($"{nameof(maxLength)}: Must be a positive number");

            _maxLength = maxLength;
        }

        public override string Key => "maxLength";

        public override object Value => _maxLength;

        public override Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            if (input == null) throw Error.ArgumentNull(nameof(input));

            var result = Assertions.Empty + this;

            if (Any.Convert(input.Value) is String serializedValue)
            {
                result += new Trace($"Maxlength validation with value '{serializedValue}'");

                if (serializedValue.Value.Length > _maxLength)
                {
                    return Task.FromResult(result + ResultAssertion.CreateFailure(new IssueAssertion(Issue.CONTENT_ELEMENT_VALUE_TOO_LONG, input.Location, $"Value '{serializedValue}' is too long (maximum length is {_maxLength}")));
                }
            }
            else return Task.FromResult(Assertions.Undecided);

            return Task.FromResult(result + Assertions.Success);
        }

    }
}