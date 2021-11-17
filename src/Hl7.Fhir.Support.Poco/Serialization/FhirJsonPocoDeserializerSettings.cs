/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER

#nullable enable

using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using System;
using System.Text.Json;

namespace Hl7.Fhir.Serialization
{
    /// <summary>
    /// Specify the optional features for Json deserialization.
    /// </summary>
    public record FhirJsonPocoDeserializerSettings
    {
        /// <summary>
        /// For performance reasons, validation of Xhtml again the rules specified in the FHIR
        /// specification for Narrative (http://hl7.org/fhir/narrative.html#2.4.0) is turned off by
        /// default. Set this property to true to perform this validation during serialization.
        /// </summary>
        public bool ValidateFhirXhtml { get; init; } = false;

        /// <summary>
        /// Validation of the string contents of Date, Time and DateTime is done during deserialization
        /// but can be turned off for modest performance gains if necessary.
        /// </summary>
        public bool SkipDateTimeValidation { get; init; } = false;

        /// <summary>
        /// If the caller will not access base64 data in the deserialized resources, base64 decoding
        /// of <see cref="Base64Binary"/> values can be turned off to increase performance.
        /// </summary>
        public bool DisableBase64Decoding { get; init; } = false;

        /// <summary>
        /// If set, this delegate is called when the deserializer fails to parse a primitive json value.
        /// </summary>
        public PrimitiveParseHandler? OnPrimitiveParseFailed { get; init; } = null;
    }

    /// <summary>
    /// A delegate for a function that can handle parsing failures for primitive types.
    /// </summary>
    /// <param name="reader">A json reader positioned on the primitive value that failed to parse.</param>
    /// <param name="originalException">The exception that the deserializer would have raised if this handler was not installed.</param>
    /// <param name="targetType">The .NET type the deserializer needs this handler to return to be able to update the POCO under construction.</param>
    /// <remarks>Returns an object if this handler succeeded in parsing, otherwise the delegate must throw an exception (which might
    /// be the <c>originalException</c>).</remarks>
    public delegate object PrimitiveParseHandler(ref Utf8JsonReader reader, Type targetType, FhirJsonException originalException);
}

#nullable restore
#endif