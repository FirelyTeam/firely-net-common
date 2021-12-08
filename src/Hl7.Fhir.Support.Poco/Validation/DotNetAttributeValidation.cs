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
        /// Validate and object and its members against any <see cref="ValidationAttribute" />s present. 
        /// Will throw when a validation error is encountered.
        /// </summary>
        public static void Validate(object value, bool recurse = false, NarrativeValidationKind narrativeValidation = NarrativeValidationKind.FhirXhtml)
        {
            var validationContext = buildContext(recurse, narrativeValidation, value);
            Validator.ValidateObject(value, validationContext, true);
        }

        /// <summary>
        /// Validate an object and its members against any <see cref="ValidationAttribute" />s present. 
        /// </summary>
        /// <remarks>If <paramref name="validationResults"/> is <c>null</c>, no errors will be returned.</remarks>
        public static bool TryValidate(object value, ICollection<ValidationResult>? validationResults = null, bool recurse = false, NarrativeValidationKind narrativeValidation = NarrativeValidationKind.FhirXhtml)
        {
            var validationContext = buildContext(recurse, narrativeValidation, value);

            // Validate the object, also calling the validators on each child property.
            var results = validationResults ?? new List<ValidationResult>();
            return Validator.TryValidateObject(value, validationContext, results, validateAllProperties: true);
        }

        private static ValidationContext buildContext(bool recurse, NarrativeValidationKind kind, object? instance)
        {
            ValidationContext newContext =
#if NET40
                new ValidationContext(instance ?? new object(), null, null);
#else
                new(instance ?? new object());
#endif

            newContext.SetValidateRecursively(recurse);
            newContext.SetNarrativeValidationKind(kind);
            return newContext;
        }

    }

}

#nullable restore