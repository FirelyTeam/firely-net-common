/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#nullable enable

using Hl7.Fhir.Validation;

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// A validator that will be run to validate values while deserializing a POCO.
    /// </summary>
    public interface IDeserializationValidator
    {
        /// <summary>
        /// Implements validation logic to be run just before the value is used to initialize
        /// the deserialized object.
        /// </summary>
        /// <param name="candidateValue">The value to be validated.</param>
        /// <param name="context">The current context of deserialization, like the path and the type under deserialization.</param>
        /// <param name="reportedErrors">null, Zero or more validation errors which will be aggregated in the final result of deserialization.</param>
        /// <param name="validatedValue">The validated value that will be used to initialize the deserialized object at this point.</param>
        /// <returns>An array with zero or more formatted strings detailing the validation issues.</returns>
        void Validate(
            object? candidateValue,
            in DeserializationContext context,
            out CodedValidationException[]? reportedErrors,
            out object? validatedValue);
    }
}

#nullable restore
