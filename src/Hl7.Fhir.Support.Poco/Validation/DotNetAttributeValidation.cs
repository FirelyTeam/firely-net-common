/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace Hl7.Fhir.Validation
{
    /// <summary>
    /// Utility methods for invoking .NET's <see cref="ValidationAttribute"/>-based validation mechanism.
    /// </summary>
    public static class DotNetAttributeValidation
    {
        /// <summary>
        /// Validate a value against a specific <see cref="ValidationAttribute" />.
        /// </summary>
        public static ValidationResult? GetValidationResult(object value, ValidationAttribute va, bool recurse = false)
        {
            var validationContext = buildContext(value);
            validationContext.SetValidateRecursively(recurse);

            return va.GetValidationResult(value, validationContext);
        }

        /// <summary>
        /// Validate a value, and its members against any <see cref="ValidationAttribute" />s present. 
        /// Will throw when a validation error is encountered.
        /// </summary>
        public static void Validate(object value, bool recurse = false)
        {
            var validationContext = buildContext(value);
            validationContext.SetValidateRecursively(recurse);

            Validator.ValidateObject(value, validationContext, true);
        }

        /// <summary>
        /// Validate a value, and its members against any <see cref="ValidationAttribute" />s present. 
        /// </summary>
        /// <remarks>If <paramref name="validationResults"/> is <c>null</c>, no errors will be returned.</remarks>
        public static bool TryValidate(object value, ICollection<ValidationResult>? validationResults = null, bool recurse = false)
        {
            var results = validationResults ?? new List<ValidationResult>();
            var validationContext = buildContext(value);
            validationContext.SetValidateRecursively(recurse);

            // Validate the object, also calling the validators on each child property.
            return Validator.TryValidateObject(value, validationContext, results, validateAllProperties: true);
        }

        /// <summary>
        /// Convenience method for creating valid <see cref="ValidationResult" />s with a formatted message.
        /// </summary>
        public static ValidationResult BuildResult(ValidationContext context, string message, params object[] messageArgs)
        {
            var resultMessage = string.Format(message, messageArgs);

            return context?.MemberName is not null
                ? new ValidationResult(resultMessage, new string[] { context.MemberName })
                : new ValidationResult(resultMessage);
        }

        private static ValidationContext buildContext(object value)
        {
#if NET40
                return new ValidationContext(value, null, null);
#else
            return new ValidationContext(value);
#endif
        }

    }

}

#nullable restore