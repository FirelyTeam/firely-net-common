/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hl7.Fhir.Validation
{
    public static class DotNetAttributeValidation
    {
        public static ValidationResult GetValidationResult(object value, ValidationAttribute va, bool recurse = false)
        {
            var validationContext = buildContext(value);
            validationContext.SetValidateRecursively(recurse);

            return va.GetValidationResult(value, validationContext);
        }

        public static void Validate(object value, bool recurse = false)
        {
            var validationContext = buildContext(value);
            validationContext.SetValidateRecursively(recurse);

            Validator.ValidateObject(value, validationContext, true);
        }

        public static bool TryValidate(object value, ICollection<ValidationResult> validationResults = null, bool recurse = false)
        {
            var results = validationResults ?? new List<ValidationResult>();
            var validationContext = buildContext(value);
            validationContext.SetValidateRecursively(recurse);

            // Validate the object, also calling the validators on each child property.
            return Validator.TryValidateObject(value, validationContext, results, validateAllProperties: true);

            // Note, if you pass a null validationResults, you will *not* get results (it's not an out param!)
        }
     

        public static ValidationResult BuildResult(ValidationContext context, string message, params object[] messageArgs)
        {
            var resultMessage = String.Format(message, messageArgs);

            if(context != null && context.MemberName != null)
                return new ValidationResult(resultMessage, new string[] { context.MemberName });
            else
                return new ValidationResult(resultMessage);
        }

        private static ValidationContext buildContext(object value = null)
        {
            #if NET40
                return new ValidationContext(value, null, null);
            #else
                return new ValidationContext(value);
            #endif
        }

    }

}
