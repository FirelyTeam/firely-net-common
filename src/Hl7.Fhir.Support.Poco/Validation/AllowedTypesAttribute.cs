/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Utility;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Hl7.Fhir.Validation
{
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class AllowedTypesAttribute : ValidationAttribute
    {
        public AllowedTypesAttribute(params Type[] types)
        {
            Types = types;
        }

        public Type[] Types { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var list = value as IEnumerable;
            ValidationResult result = ValidationResult.Success;

            // Avoid interpreting this as a collection just because string is IEnumerable<char>.
            if (list != null && !(value is string))
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

        private ValidationResult validateValue(object item, ValidationContext context)
        {
            if (item != null)
            {
                if (!IsAllowedType(item.GetType()))
                    return DotNetAttributeValidation.BuildResult(context, "Value is of type {0}, which is not an allowed choice", item.GetType());
            }

            return ValidationResult.Success;
        }

        public bool IsAllowedType(Type t) => Types.Any(type => type.IsAssignableFrom(t));
    }
}
