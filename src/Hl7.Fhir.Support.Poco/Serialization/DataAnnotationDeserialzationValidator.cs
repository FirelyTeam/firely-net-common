/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Introspection;
using Hl7.Fhir.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Hl7.Fhir.Serialization
{
    public class DataAnnotationDeserialzationValidator : IDeserializationValidator
    {
        public static readonly DataAnnotationDeserialzationValidator Default = new();

        /// <summary>
        /// For performance reasons, validation of Xhtml again the rules specified in the FHIR
        /// specification for Narrative (http://hl7.org/fhir/narrative.html#2.4.0) is turned off by
        /// default. Set this property to any other value than <see cref="NarrativeValidationKind.None"/>
        /// to perform validation.
        /// </summary>
        public NarrativeValidationKind NarrativeValidation { get; } = NarrativeValidationKind.None;

        /// <summary>
        /// A list of types (generally subclasses of <see cref="ValidationAttribute"/>) for the attributes
        /// that should not be invoked while doing validation.
        /// </summary>
        public Type[]? Excludes { get; } = null;

        public DataAnnotationDeserialzationValidator(
            NarrativeValidationKind narrativeValidation = NarrativeValidationKind.None,
            Type[]? excludes = null)
        {
            NarrativeValidation = narrativeValidation;
            Excludes = excludes;
        }

        /// <inheritdoc cref="IDeserializationValidator.Validate(object?, in DeserializationContext, out CodedValidationException[], out object?)"/>
        public void Validate(object? candidateValue, in DeserializationContext context, out CodedValidationException[]? reportedErrors, out object? validatedValue)
        {
            // We are not rewriting the value, so set it immediately
            validatedValue = candidateValue;

            // Avoid allocation of a list for every validation until we really have something to report.
            List<CodedValidationException>? errors = null;

            var validationContext = new ValidationContext(candidateValue ?? new object())
                .SetValidateRecursively(false)
                .SetNarrativeValidationKind(NarrativeValidation);

            foreach (var va in context.ElementMapping.ValidationAttributes)
            {
                // The ElementAttribute does not add to this elements validation - it's only used
                // to extend .NETs property validation into nested values (which we have already validated
                // while parsing bottom up).
                var skip = va is FhirElementAttribute ||
                    Excludes?.Contains(va.GetType()) == true;

                if (!skip)
                {
                    if (va.GetValidationResult(candidateValue, validationContext) is object vr)
                    {
                        if (vr is CodedValidationResult cvr)
                            addError(cvr.ValidationException);
                        else
                            throw new InvalidOperationException($"Validation attributes should return a {nameof(CodedValidationResult)}.");

                        void addError(CodedValidationException e)
                        {
                            if (errors is null) errors = new();
                            errors.Add(e);
                        }
                    }
                }
            }

            reportedErrors = errors?.ToArray();
        }
    }
}

#nullable restore