/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Introspection;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

#nullable enable

namespace Hl7.Fhir.Validation
{
    /// <summary>
    /// Validates the type of a property against the allowed type choices.
    /// </summary>
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class AllowedTypesAttribute : ValidationAttribute
    {
        public AllowedTypesAttribute(params Type[] types)
        {
            Types = types;
        }

        /// <summary>
        /// The list of types that are allowed for the instance.
        /// </summary>
        public Type[] Types { get; set; }

        /// <inheritdoc />
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success;

            var result = ValidationResult.Success;

            // Avoid interpreting this as a collection just because string is IEnumerable<char>.
            if (value is ICollection list && value is not string)
            {
                foreach (var item in list)
                {
                    result = validateValue(item, validationContext);
                    if (result != ValidationResult.Success) break;
                }
            }
            else
            {
                result = validateValue(value, validationContext);
            }

            return result;
        }

        private ValidationResult? validateValue(object? item, ValidationContext context) =>
            item is null || IsAllowedType(item.GetType())
                ? ValidationResult.Success
                : DotNetAttributeValidation.BuildResult(context, "Value is of type '{0}', which is not an allowed choice.",
                        ModelInspector.GetClassMappingForType(item.GetType())?.Name ?? item.GetType().Name);

        /// <summary>
        /// Determine whether the given type is allowed according to this attribute.
        /// </summary>
        public bool IsAllowedType(Type t) => Types.Any(type => type.GetTypeInfo().IsAssignableFrom(t));
    }
}

#nullable restore