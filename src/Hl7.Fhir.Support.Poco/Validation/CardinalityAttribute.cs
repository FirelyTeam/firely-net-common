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

#nullable enable

namespace Hl7.Fhir.Validation
{
    /// <summary>
    /// Validates a List instance against the cardinality min/max rules.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class CardinalityAttribute : ValidationAttribute
    {
        public CardinalityAttribute()
        {
            Min = 0;
            Max = 1;
        }

        /// <summary>
        /// The minimum number of occurrences.
        /// </summary>
        public int Min { get; set; }

        /// <summary>
        /// The maximum number of occurences. Use <c>-1</c> for unlimited.
        /// </summary>
        public int Max { get; set; }

        /// <inheritdoc/>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
                return (Min == 0) ? ValidationResult.Success :
                    DotNetAttributeValidation.BuildResult(validationContext, "Element with min. cardinality {0} cannot be null.", Min);

            var count = 1;

            if (value is IList list && !ReflectionHelper.IsArray(value))
            {
                foreach (var elem in list)
                    if (elem == null) return DotNetAttributeValidation.BuildResult(validationContext, "Repeating element cannot have empty/null values.");
                count = list.Count;
            }

            if (count < Min) return DotNetAttributeValidation.BuildResult(validationContext, "Element has {0} elements, but min. cardinality is {1}.", count, Min);

            if (Max != -1 && count > Max) return DotNetAttributeValidation.BuildResult(validationContext, "Element has {0} elements, but max. cardinality is {1}.", count, Max);

            return ValidationResult.Success;
        }
    }
}

#nullable restore