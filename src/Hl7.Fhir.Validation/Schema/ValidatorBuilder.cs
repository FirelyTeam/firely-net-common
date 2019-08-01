/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Schema;
using System.Collections.Generic;

namespace Hl7.Fhir.Specification.Schema
{
    public class ValidatorBuilder
    {
        private readonly List<IValidatable> _validators = new List<IValidatable>();
        private readonly ValidationContext _validationContext;

        public ValidatorBuilder(ValidationContext vc)
        {
            _validationContext = vc;
        }

        public Assertions Validate(ITypedElement input)
        {
            var assertions = Assertions.Empty;
            try
            {
                foreach (var validator in _validators)
                {
                    assertions += validator.Validate(input, _validationContext);
                }
            }
            catch (IncorrectElementDefinitionException iede)
            {
                assertions += Assertions.Failure;
                //outcome.AddIssue("Incorrect ElementDefinition: " + iede.Message, Issue.PROFILE_ELEMENTDEF_INCORRECT);
            }

            return assertions;
        }


        public void Add(IValidatable validator)
        {
            _validators.Add(validator);
        }
    }
}
