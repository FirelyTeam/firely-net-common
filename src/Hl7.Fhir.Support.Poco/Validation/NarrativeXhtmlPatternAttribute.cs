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
    /// Validates an xhtml value against the FHIR rules for xhtml.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class NarrativeXhtmlPatternAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
            IsValid(value, NarrativeValidationKind.FhirXhtml);

        /// <summary>
        /// Validates whether the value is a string of well-formatted Xml.
        /// </summary>
        public ValidationResult? IsValid(object? value, NarrativeValidationKind kind)
        {
            if (value is null) return ValidationResult.Success;

            if (value is string xml)
            {
                return kind switch
                {
                    NarrativeValidationKind.None => ValidationResult.Success,
                    NarrativeValidationKind.Xml => XHtml.IsValidXml(xml, out var error) ?
                            ValidationResult.Success :
                            new ValidationResult("Value is not well-formatted Xml: " + error),
                    NarrativeValidationKind.FhirXhtml => XHtml.IsValidNarrativeXhtml(xml, out var errors) ?
                                ValidationResult.Success :
                                new ValidationResult("Xml can not be parsed or is not valid according to the (limited) FHIR scheme: " +
                                string.Join(", ", errors)),
                    _ => throw new NotSupportedException($"Encountered unknown narrative validation kind {kind}.")
                };
            }
            else
                throw new ArgumentException($"{nameof(NarrativeXhtmlPatternAttribute)} attributes can only be applied to string properties.");
        }
    }
}


#nullable restore