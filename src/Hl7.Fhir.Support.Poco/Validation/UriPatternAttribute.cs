/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Model;
using System;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace Hl7.Fhir.Validation
{
    /// <summary>
    /// Validates an Uri value against the FHIR rules for Uri.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class UriPatternAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
            value switch
            {
                null => ValidationResult.Success,
                string s when FhirUri.IsValidValue(s) => ValidationResult.Success,
                string s => DotNetAttributeValidation.BuildResult(validationContext, "Uri uses a 'urn:oid' or 'urn:uuid' scheme, but the syntax '{0}' is incorrect.", s),
                _ => throw new ArgumentException($"{nameof(UriPatternAttribute)} attributes can only be applied to string properties.")
            };
    }
}

#nullable restore
