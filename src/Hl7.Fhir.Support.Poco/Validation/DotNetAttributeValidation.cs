/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Hl7.Fhir.Validation
{
    public static class DotNetAttributeValidation
    {
        public static ValidationContext BuildContext(object value=null)
        {
#if NET40
            return new ValidationContext(value, null, null);
#else
            return new ValidationContext(value);
#endif
        }

        public static void Validate(object value, bool recurse = false, Func<string, Resource> resolver = null)
        {
            if (value == null) throw new ArgumentNullException("value");
            //    assertSupportedInstanceType(value);

            var validationContext = BuildContext(value);
            validationContext.SetValidateRecursively(recurse);
            validationContext.SetResolver(resolver);

            Validator.ValidateObject(value, validationContext, true);
        }

        public static bool TryValidate(object value, ICollection<ValidationResult> validationResults = null, bool recurse = false, Func<string,Resource> resolver=null)
        {
            if (value == null) throw new ArgumentNullException("value");
          // assertSupportedInstanceType(value);

            var results = validationResults ?? new List<ValidationResult>();
            var validationContext = BuildContext(value);
            validationContext.SetValidateRecursively(recurse);
            validationContext.SetResolver(resolver);
            return Validator.TryValidateObject(value, validationContext, results, true);

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
    }

}
